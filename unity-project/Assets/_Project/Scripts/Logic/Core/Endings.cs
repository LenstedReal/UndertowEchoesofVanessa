namespace Undertow.Logic
{
    /// <summary>
    /// Story Bible §6. The ending follows the player's behaviour, never a menu choice.
    ///  Answer   : answered in Act III + at least 4 of Vanessa's 6 drawings + chose Echo's (wrong) lullaby.
    ///  Surface  : answered in Act III, but the drawing or the lullaby condition failed.
    ///  Undertow : stayed silent in Act III, or chose the Mimic's (correct) lullaby.
    /// </summary>
    public static class Endings
    {
        public const int DrawingsForAnswer = 4;

        public static EndingId Resolve(bool answeredInAct3, int drawingsFound, AltarChoice altar)
        {
            if (!answeredInAct3 || altar == AltarChoice.Mimic) return EndingId.Undertow;
            if (altar == AltarChoice.Echo && drawingsFound >= DrawingsForAnswer) return EndingId.Answer;
            return EndingId.Surface;
        }

        public static EndingId Resolve(GameState s) => Resolve(s.AnsweredInAct3, s.DrawingCount, s.Altar);

        public static string Key(EndingId e)
        {
            switch (e)
            {
                case EndingId.Answer: return "answer";
                case EndingId.Surface: return "surface";
                case EndingId.Undertow: return "undertow";
                default: return "none";
            }
        }
    }
}
