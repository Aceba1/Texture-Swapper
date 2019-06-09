using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Texture_Swapper
{
    static class UnstableWrap
    {
        public static void AddMoreInfoToSkin(CorporationSkinInfo skinInfo, int ID, FactionSubTypes faction)
        {
            skinInfo.m_SkinUniqueID = ID;
            skinInfo.m_Corporation = faction;
        }
    }
}
