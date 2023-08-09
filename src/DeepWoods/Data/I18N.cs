using StardewModdingAPI;
using StardewValley;

namespace DeepWoodsMod
{
    public class I18N
    {
        private static ITranslationHelper I18n;

        private static string Get(string key)
        {
            string text = I18n.Get(key);

            if (text.Contains("^"))
            {
                string[] array = text.Split('^');
                var player = Game1.player;
                if (player != null)
                {
                    return player.IsMale ? array[0] : array[1];
                }
                else
                {
                    return array[1];
                }
            }

            return text;
        }

        public static string ExcaliburDisplayName => Get("excalibur.name");
        public static string ExcaliburDescription => Get("excalibur.description");
        public static string WoodsObeliskDisplayName => Get("woods-obelisk.name");
        public static string WoodsObeliskDescription => Get("woods-obelisk.description");
        public static string EasterEggDisplayName => Get("easter-egg.name");
        public static string EasterEggHatchedMessage => Get("easter-egg.hatched-message");
        public static string LostMessage => Get("lost-message");
        public static string WoodsObeliskWizardMailMessage => Get("woods-obelisk.wizard-mail");
        public static string HealingFountainDrinkMessage => Get("healing-fountain.drink-message");
        public static string ExcaliburNopeMessage => Get("excalibur.nope-message");
        public static string MessageBoxOK => Get("messagebox.ok");
        public static string MaxHousePuzzleNopeMessage => Get("maxhouse.puzzle.nope");

        public static string OrbStoneTouchQuestion => Get("orb-stone.question");
        public static string OrbStoneTouchYes => Get("orb-stone.yes");
        public static string OrbStoneTouchNope => Get("orb-stone.no");
        public static string OrbStoneTouchMessage => Get("orb-stone.touch-message");
        public static string OrbStoneTouchMessageNoOrb => Get("orb-stone.touch-message-no-orb");

        public static string BooksMessage => Get("maxhouse.books.question");
        public static string BooksAnswerRead => Get("maxhouse.books.answer.read");
        public static string BooksAnswerNevermind => Get("maxhouse.books.answer.nevermind");
        public static string BooksInteresting => Get("maxhouse.books.interesting");


        public static string StuffMessage => Get("maxhouse.stuff.question");
        public static string StuffAnswerSearch => Get("maxhouse.stuff.answer.search");
        public static string StuffAnswerNevermind => Get("maxhouse.stuff.answer.nevermind");
        public static string StuffNothing => Get("maxhouse.stuff.nothing");

        public static string QuestsEmptyMessage => Get("maxhouse.quests.empty");
        public static string ShopEmptyMessage => Get("maxhouse.shop.empty");

        public static string BigWoodenSignMessage => Get("bigsign.message");


        public static void Init(ITranslationHelper i18n)
        {
            I18n = i18n;
        }

        public class SignTexts
        {
            public readonly static string[] textIDs = new string[]
            {
                "sign.text.welcome",
                "sign.text.random.1",
                "sign.text.random.2",
                "sign.text.random.3",
                "sign.text.random.4",
                "sign.text.random.5",
                "sign.text.random.6",
                "sign.text.random.7",
                "sign.text.random.8",
            };

            public static string Get(int index)
            {
                return I18N.Get(textIDs[index]);
            }
        }
    }
}
