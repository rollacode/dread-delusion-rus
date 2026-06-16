using System;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using TMPro;

namespace DDRuFont
{
    [BepInPlugin("com.dyusha.ddrufont", "DD Russian Font", "1.0.0")]
    public class Plugin : BasePlugin
    {
        internal static ManualLogSource Logger;
        public override void Load()
        {
            Logger = Log;
            Log.LogInfo("DDRuFont loading...");
            ClassInjector.RegisterTypeInIl2Cpp<FontInjector>();
            var go = new GameObject("DDRuFontInjector");
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<FontInjector>();
        }
    }

    public class FontInjector : MonoBehaviour
    {
        public FontInjector(IntPtr ptr) : base(ptr) { }

        private bool _loaded;
        private float _t;
        private int _reapply;
        private TMP_FontAsset _ru;

        // auto-size: shrink only text that overflows; text that fits stays unchanged
        private float _asT;

        void Update()
        {
            _t += Time.deltaTime;
            _asT += Time.deltaTime;

            if (!_loaded)
            {
                if (_t < 3f) return;
                _t = 0f;
                try { LoadFont(); }
                catch (Exception e) { Plugin.Logger.LogError("DDRuFont load error: " + e); _loaded = true; }
                return;
            }

            // Re-apply fallback a few times as new TMP fonts get loaded across scenes
            if (_ru != null && _t >= 5f && _reapply < 12)
            {
                _t = 0f;
                _reapply++;
                try { ApplyFallback(); } catch (Exception e) { Plugin.Logger.LogWarning("reapply: " + e.Message); }
            }

            // continuously enable auto-size on new TMP text (item descriptions etc. spawn dynamically)
            if (_asT >= 0.4f)
            {
                _asT = 0f;
                try { PrimaryFontPass(); } catch (Exception e) { Plugin.Logger.LogWarning("primaryfont: " + e.Message); }
                try { AutoSizePass(); } catch (Exception e) { Plugin.Logger.LogWarning("autosize: " + e.Message); }
            }
        }

        static bool HasCyrillic(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c >= 'Ѐ' && c <= 'ӿ') return true;
            }
            return false;
        }

        // Promote the RU font from fallback to PRIMARY on any text that contains Cyrillic but
        // is drawn with a different base font (e.g. the pixel/DOS font in the equipment menu).
        // Without this the Cyrillic glyphs are pulled from the alagard fallback while the rest of
        // the label stays in the base font -> mixed look. Forcing the base font to RU makes the
        // whole string render in alagard, consistent with the rest of the game.
        void PrimaryFontPass()
        {
            if (_ru == null) return;
            int swapped = 0;
            var all = Resources.FindObjectsOfTypeAll<TMP_Text>();
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t == null) continue;
                if (t.font == _ru) continue;          // already RU
                if (!HasCyrillic(t.text)) continue;    // no Russian text -> leave base font (numbers/icons untouched)
                t.font = _ru;
                swapped++;
            }
            if (swapped > 0) Plugin.Logger.LogInfo("DDRuFont: promoted RU to primary on " + swapped + " text(s)");
        }

        void AutoSizePass()
        {
            var all = Resources.FindObjectsOfTypeAll<TMP_Text>();
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t == null) continue;
                if (t.enableAutoSizing) continue;   // already auto-sizing (pre-existing or set by us) -> skip
                float fs = t.fontSize;
                if (fs <= 0f) continue;
                // keep current size as the max; allow shrink down to fit -> overflow text scales down, fitting text unchanged
                t.enableAutoSizing = true;
                t.fontSizeMax = fs;
                t.fontSizeMin = Mathf.Max(9f, fs * 0.5f);
            }
        }

        void LoadFont()
        {
            string path = BepInEx.Paths.PluginPath + "\\alagard.bundle";
            if (!System.IO.File.Exists(path)) { Plugin.Logger.LogError("bundle missing: " + path); _loaded = true; return; }

            var ab = AssetBundle.LoadFromFile(path);
            if (ab == null) { Plugin.Logger.LogError("bundle load null"); _loaded = true; return; }

            var assets = ab.LoadAllAssets();
            for (int i = 0; i < assets.Length; i++)
            {
                var o = assets[i];
                if (o == null) continue;
                var fa = o.TryCast<TMP_FontAsset>();
                if (fa != null) { _ru = fa; break; }
            }
            if (_ru == null) { Plugin.Logger.LogError("no TMP_FontAsset in bundle"); _loaded = true; return; }

            _ru.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(_ru);
            Plugin.Logger.LogInfo("DDRuFont: loaded fallback font '" + _ru.name + "' from bundle");

            // Make the fallback material use the GAME's TMP shader (2019.4 shader may not work in 2022.3)
            try
            {
                var all = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                Shader gameShader = null;
                for (int i = 0; i < all.Length; i++)
                {
                    var fa = all[i];
                    if (fa == null || fa == _ru) continue;
                    if (fa.material != null && fa.material.shader != null) { gameShader = fa.material.shader; break; }
                }
                if (gameShader != null && _ru.material != null)
                {
                    _ru.material.shader = gameShader;
                    Plugin.Logger.LogInfo("DDRuFont: set fallback material shader -> " + gameShader.name);
                }
                else Plugin.Logger.LogWarning("DDRuFont: could not source game TMP shader (gameShader=" + (gameShader != null) + ")");
            }
            catch (Exception e) { Plugin.Logger.LogWarning("shader swap: " + e.Message); }

            _loaded = true;
            _t = 5f;
            ApplyFallback();
        }

        void ApplyFallback()
        {
            // global fallback
            var glist = TMP_Settings.fallbackFontAssets;
            if (glist != null)
            {
                bool has = false;
                for (int i = 0; i < glist.Count; i++) if (glist[i] == _ru) { has = true; break; }
                if (!has) { glist.Add(_ru); Plugin.Logger.LogInfo("DDRuFont: + global fallback (count=" + glist.Count + ")"); }
            }

            // per-font fallback tables
            int patched = 0;
            var all = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            for (int i = 0; i < all.Length; i++)
            {
                var fa = all[i];
                if (fa == null || fa == _ru) continue;
                var tbl = fa.fallbackFontAssetTable;
                if (tbl == null) continue;
                bool has = false;
                for (int j = 0; j < tbl.Count; j++) if (tbl[j] == _ru) { has = true; break; }
                if (!has) { tbl.Add(_ru); patched++; }
            }
            if (patched > 0) Plugin.Logger.LogInfo("DDRuFont: + " + patched + " per-font fallbacks (total fonts=" + all.Length + ")");
        }
    }
}
