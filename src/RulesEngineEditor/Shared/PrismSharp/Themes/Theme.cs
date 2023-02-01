// This file is a part of PrismSharp Library by Tomas Kubec
// based on PrismJS, https://prismjs.com/
// Distributed under MIT license - see license.txt
//

using Newtonsoft.Json;
using Orionsoft.PrismSharp.Tokenizing;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Orionsoft.PrismSharp.Themes
{
    /// <summary>
    /// Object containing styling for tokens based on currently loaded theme
    /// </summary>
    public class Theme
    {
        [JsonProperty("styles")]
        internal List<ThemeStyle> Styles { get; set; }

        internal Theme()
        {
        }

        /// <summary>
        /// Loads built-in theme by provided name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Theme Load(ThemeNames name)
        {
            var themes = new Dictionary<ThemeNames, string>
            {
                { ThemeNames.A11yDark, "a11y-dark.json"},
                { ThemeNames.AtomDark, "atom-dark.json"},
                { ThemeNames.AteliersulphurpoolLight, "base16-ateliersulphurpool.light.json"},
                { ThemeNames.Cb, "cb.json"},
                { ThemeNames.ColdarkCold, "coldark-cold.json"},
                { ThemeNames.ColdarkDark, "coldark-dark.json"},
                { ThemeNames.Coy, "coy.json"},
                { ThemeNames.Darcula, "darcula.json"},
                { ThemeNames.Dark, "dark.json"},
                { ThemeNames.Dracula, "dracula.json"},
                { ThemeNames.DuotoneDark, "duotone-dark.json"},
                { ThemeNames.DuotoneEarth, "duotone-earth.json"},
                { ThemeNames.DuotoneForest, "duotone-forest.json"},
                { ThemeNames.DuotoneLight, "duotone-light.json"},
                { ThemeNames.DuotoneSea, "duotone-sea.json"},
                { ThemeNames.DuotoneSpace, "duotone-space.json"},
                { ThemeNames.Funky, "funky.json"},
                { ThemeNames.Ghcolors, "ghcolors.json"},
                { ThemeNames.GruvboxDark, "gruvbox-dark.json"},
                { ThemeNames.GruvboxLight, "gruvbox-light.json"},
                { ThemeNames.HoliTheme, "holi-theme.json"},
                { ThemeNames.Hopscotch, "hopscotch.json"},
                { ThemeNames.Lucario, "lucario.json"},
                { ThemeNames.MaterialDark, "material-dark.json"},
                { ThemeNames.MaterialLight, "material-light.json"},
                { ThemeNames.MaterialOceanic, "material-oceanic.json"},
                { ThemeNames.NightOwl, "night-owl.json"},
                { ThemeNames.Nord, "nord.json"},
                { ThemeNames.Okaidia, "okaidia.json"},
                { ThemeNames.OneDark, "one-dark.json"},
                { ThemeNames.OneLight, "one-light.json"},
                { ThemeNames.Pojoaque, "pojoaque.json"},
                { ThemeNames.Prism, "prism.json"},
                { ThemeNames.ShadesOfPurple, "shades-of-purple.json"},
                { ThemeNames.SolarizedDarkAtom, "solarized-dark-atom.json"},
                { ThemeNames.Solarizedlight, "solarizedlight.json"},
                { ThemeNames.Synthwave84, "synthwave84.json"},
                { ThemeNames.Tomorrow, "tomorrow.json"},
                { ThemeNames.Twilight, "twilight.json"},
                { ThemeNames.Vs, "vs.json"},
                { ThemeNames.VscDarkPlus, "vsc-dark-plus.json"},
                { ThemeNames.Xonokai, "xonokai.json"},
                { ThemeNames.ZTouch, "z-touch.json"}
            };
            var dir = Path.Combine(Utils.AssemblyDirectory, "data/themes/");

            return LoadFromFile(Path.Combine(dir, themes[name]));
        }

        /// <summary>
        /// Loads custom theme from file. Only use for own themes, not for the built-in ones.
        /// </summary>
        public static Theme LoadFromFile(string themeFile)
        {
            var jsonText = File.ReadAllText(themeFile);
            return JsonConvert.DeserializeObject<Theme>(jsonText);
        }

        /// <summary>
        /// Gets styling for the token, according to the used language
        /// </summary>
        public ThemeStyle GetTokenStyle(Token token, string language)
        {
            if (token == null) return null;

            language = string.IsNullOrEmpty(language) ? null : language;

            var general = GetTokenStyleLang(token, null);
            var langSpecific = GetTokenStyleLang(token, language);

            ThemeStyle res;

            if (langSpecific != null && general != null)
            {
                res = langSpecific.MergeWith(general);
            }
            else
            {
                res = general ?? langSpecific;
            }

            return res;
        }

        /// <summary>
        /// Gets styling for the entire highlighted source code, according to the used language
        /// </summary>
        public ThemeStyle GetDocumentStyle(string language)
        {
            language = string.IsNullOrEmpty(language) ? null : language;

            var general = Styles.FirstOrDefault(x => x.Type == "~" && x.Language == null);
            var langSpecific = Styles.FirstOrDefault(x => x.Type == "~" && x.Language == language);

            if (langSpecific == null) return general;
            if (general == null) return langSpecific;

            var res = langSpecific.MergeWith(general);
            return res;
        }

        private ThemeStyle GetTokenStyleLang(Token p, string language)
        {
            var res = Styles.FirstOrDefault(x => x.Type == p.Type && x.Language == language);
            if (res != null) return res;

            if (p.Aliases != null)
            {
                foreach (var alias in p.Aliases)
                {
                    res = Styles.FirstOrDefault(x => x.Type == alias && x.Language == language);
                    if (res != null) return res;
                }
            }
            return null;
        }
    }
}