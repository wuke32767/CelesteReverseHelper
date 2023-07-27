namespace Celeste.Mod.ReverseHelper.Libraries
{
    public struct FlagMatch
    {
        private bool invert;
        private string flag;

        public FlagMatch(string s)
        {
            if (s.Length != 0 && s[0] == '!')
            {
                invert = true;
                flag = s.Remove(0, 1);
            }
            else
            {
                invert = false;
                flag = s;
            }
        }

        public static implicit operator FlagMatch(string s) => new FlagMatch(s);

        public bool IsMatch(Level/*unlikely nullable*/ lv, bool v)
        {
            if (Empty())
            {
                return v;
            }
            return (lv?.Session.GetFlag(flag) ?? invert) != invert;
        }

        internal bool Empty()
        {
            if (flag.Length == 0)
            {
                return true;
            }
            return false;
        }
    }
}