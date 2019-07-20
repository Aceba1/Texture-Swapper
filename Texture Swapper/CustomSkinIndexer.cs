using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Texture_Swapper
{
    class CustomSkinIndexer : Module
    {
        static Type Type = typeof(CustomSkinIndexer);
        private CustomSkinIndexer()
        {
            base.block.serializeEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
            //base.block.serializeTextEvent.Subscribe(new Action<bool, TankPreset.BlockSpec>(this.OnSerialize));
        }

        private void OnSerialize(bool saving, TankPreset.BlockSpec blockSpec)
        {
            var corp = ManSpawn.inst.GetCorporation(block.BlockType);
            byte f = (byte)corp;
            if (Main.SwapDictByteToID.ContainsKey(f))
            {
                if (saving)
                {
                    try
                    {
                        if (Main.SwapDictByteToID.ContainsKey(f) && Main.SwapDictByteToID[f].TryGetValue(block.GetSkinIndex(), out string NewSkinName))
                        {
                            blockSpec.Store(Type, "SkinName", NewSkinName);
                        }
                        else if (block.GetSkinIndex() != 0)
                        {
                            Console.WriteLine();
                        }
                    }
                    catch { Console.WriteLine("TextureSwapper [Save] FAILED"); }
                }
                else
                {
                    try
                    {
                        string str = null;

                        CustomSkinIndexer.SerialData serialData2 = Module.SerialData<CustomSkinIndexer.SerialData>.Retrieve(blockSpec.saveState);
                        if (serialData2 != null)
                        {
                            str = serialData2.SkinName;
                        }
                        else
                        {
                            string prefix = string.Format("{0} {1} ", Type, "SkinName");
                            try
                            {
                                str = blockSpec.textSerialData.Single((string s) => s.StartsWith(prefix)).Substring(prefix.Length);
                            }
                            catch { }
                        }

                        if (!string.IsNullOrEmpty(str) && Main.SwapDictIDToByte.ContainsKey(f) && Main.SwapDictIDToByte[f].TryGetValue(str, out byte Index))
                        {
                            block.SetSkinIndex(Index);
                        }
                    }
                    catch { Console.WriteLine("TextureSwapper [load] FAILED"); }
                }
            }
            new CustomSkinIndexer.SerialData().Remove(blockSpec.saveState);
        }

        [Serializable]
        private new class SerialData : Module.SerialData<CustomSkinIndexer.SerialData>
        {
            public string SkinName;
        }
    }
}
