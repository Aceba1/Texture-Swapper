using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Texture_Swapper
{
    class CustomSkinIndexer : Module
    {
        private CustomSkinIndexer()
        {
            base.block.serializeEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
            base.block.serializeTextEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
        }

        private void OnSerialize(bool saving, TankPreset.BlockSpec blockSpec)
        {
            try
            {
                if (saving)
                {
                    string SkinName = null;
                    if (Main.SwapDictByteToID.TryGetValue(new Main.BytePair() { Faction = (byte)ManSpawn.inst.GetCorporation(block.BlockType), ID = block.GetSkinIndex() }, out string NewSkinName)) 
                    {
                        SkinName = NewSkinName;
                    }
                    CustomSkinIndexer.SerialData serialData = new CustomSkinIndexer.SerialData()
                    {
                        SkinName = SkinName
                    };
                    serialData.Store(blockSpec.saveState);
                }
                else
                {
                    CustomSkinIndexer.SerialData serialData2 = Module.SerialData<CustomSkinIndexer.SerialData>.Retrieve(blockSpec.saveState);
                    if (serialData2 != null)
                    {
                        if (!string.IsNullOrEmpty(serialData2.SkinName))
                        {
                            if (Main.SwapDictIDToByte.TryGetValue(serialData2.SkinName, out var Index))
                            {
                                block.SetSkinIndex(Index.ID);
                            }
                            else
                            {
                                block.SetSkinIndex(0);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        [Serializable]
        private new class SerialData : Module.SerialData<CustomSkinIndexer.SerialData>
        {
            public string SkinName;
        }
    }
}
