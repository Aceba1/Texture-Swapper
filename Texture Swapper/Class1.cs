using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using UnityEngine;
using System.IO;

namespace Texture_Swapper
{
    /*
     * ManCustomSkins.m_SkinTextures
     * ManCustomSkins.m_SkinInfos
     * ManTechMaterialSwap.m_MaterialsToSwap 
     */
    public static class Main
    {
        public struct BytePair
        {
            public byte Faction;
            public byte ID;
        }
        internal static Dictionary<BytePair, string> SwapDictByteToID;
        internal static Dictionary<string, BytePair> SwapDictIDToByte;

        static List<CustomSkin> CustomSkins = new List<CustomSkin>();

        public static void Init()
        {
            var harmony = HarmonyInstance.Create("aceba1.textureswapper");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            LoadTextures();
        }

        private static class Patches
        {
            [HarmonyPatch(typeof(TankBlock), "OnPool")]
            static class TankBlock_Construct
            {
                static void Prefix(TankBlock __instance)
                {
                    __instance.gameObject.AddComponent<CustomSkinIndexer>();
                }
            }
        }

    static Texture2D ImageFromFile(byte[] DATA)
        {
            Texture2D texture;
            texture = new Texture2D(2, 2);
            texture.LoadImage(DATA);
            return texture;
        }

        static Type TFactionSubTypes = typeof(FactionSubTypes);

        static void LoadTextures()
        {
            SwapDictByteToID = new Dictionary<BytePair, string>();
            SwapDictIDToByte = new Dictionary<string, BytePair>();

            var dir = new DirectoryInfo(Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, "../../../"));
            string TexPath = Path.Combine(dir.FullName, "Custom Textures");
            try
            {
                if (!Directory.Exists(TexPath))
                {
                    Directory.CreateDirectory(TexPath);
                    Console.WriteLine("Created Custom Textures folder");
                }
                string path = Path.Combine(TexPath, "GSO.ExamplePack.tsmod");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    var ndir = Directory.GetFiles(Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, "../example/gso"));
                    Console.WriteLine("Created directory at " + path + ", getting " + ndir.Length + " files");
                    foreach (string file in ndir)
                    {
                        FileInfo F = new FileInfo(file);
                        File.Copy(file, Path.Combine(path, F.Name));
                    }
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("Could not access \"" + TexPath + "\"!\n" + E.Message);
            }
            Console.WriteLine("TextureSwapper: Begin");
            foreach (var texpack in new DirectoryInfo(TexPath).EnumerateDirectories())
            {
                try
                {
                    string Name = texpack.Name;
                    int lastindex = Name.LastIndexOf('.');
                    int firstindex = Name.IndexOf('.');
                    Console.Write("\nReached " + texpack.Name);
                    if (lastindex == -1 || firstindex == lastindex)
                    {
                        Console.WriteLine("\n" + texpack.FullName + " does not have the proper naming scheme! (Folder must be named as Corp.Texture Name.Author, for example, GSO.Gold.1249 )");
                        continue;
                    }
                    string Prefix = Name.Substring(0, firstindex);
                    Console.Write(", Faction:" + Prefix);
                    FactionSubTypes Faction = FactionSubTypes.NULL;
                    try
                    {
                        Faction = (FactionSubTypes)Enum.Parse(TFactionSubTypes, Prefix, true);
                    }
                    catch
                    {
                        Console.WriteLine("Could not find Corporation! The valid prefixes are:");
                        foreach (var name in Enum.GetNames(TFactionSubTypes))
                        {
                            Console.WriteLine(" " + name);
                        }
                    }
                    Console.WriteLine(", FactionID:" + (int)Faction);
                    string fpath = Path.Combine(texpack.FullName, "1.png");
                    Texture2D Albedo = new Texture2D(2, 2);
                    if (File.Exists(fpath))
                    {
                        Albedo = ImageFromFile(System.IO.File.ReadAllBytes(fpath));
                        Console.Write("Albedo, ");
                    }
                    else
                    {
                        Console.WriteLine("Couldn't find Albedo (1.png), ");
                    }
                    fpath = Path.Combine(texpack.FullName, "2.png");
                    Texture2D Metallic = new Texture2D(2, 2);
                    if (File.Exists(fpath))
                    {
                        Metallic = ImageFromFile(System.IO.File.ReadAllBytes(fpath));
                        Console.Write("Metallic, ");
                    }
                    else
                    {
                        Console.WriteLine("Couldn't find Metallic (2.png), ");
                    }
                    fpath = Path.Combine(texpack.FullName, "3.png");
                    Texture2D Emission = new Texture2D(2, 2);
                    if (File.Exists(fpath))
                    {
                        Emission = ImageFromFile(System.IO.File.ReadAllBytes(fpath));
                        Console.Write("Emission, ");
                    }
                    else
                    {
                        Console.Write("Couldn't find Emission (2.png), ");
                    }
                    fpath = Path.Combine(texpack.FullName, "preview.png");
                    Texture2D Preview = null;
                    if (File.Exists(fpath))
                    {
                        Preview = ImageFromFile(System.IO.File.ReadAllBytes(fpath));
                        Console.Write("Preview, ");
                    }
                    fpath = Path.Combine(texpack.FullName, "button.png");
                    Texture2D Button = null;
                    if (File.Exists(fpath))
                    {
                        Button = ImageFromFile(System.IO.File.ReadAllBytes(fpath));
                        Console.Write("Button, ");
                    }
                    fpath = Path.Combine(texpack.FullName, "buttonmini.png");
                    Texture2D ButtonMini = null;
                    if (File.Exists(fpath))
                    {
                        Button = ImageFromFile(System.IO.File.ReadAllBytes(fpath));
                        Console.Write("ButtonMini, ");
                    }
                    CustomSkins.Add(new CustomSkin(Name, Name, Faction, Albedo, Metallic, Emission, Preview, Button, ButtonMini));
                    Console.WriteLine("processing... \nAdded skin to list!");
                }
                catch (Exception E)
                {
                    Console.WriteLine("\nEXCEPTION! " + E.ToString());
                }
            }
            Console.WriteLine("\nReached all textures, applying skins...");
            Type TManCustomSkins = typeof(ManCustomSkins);
            var m_SkinInfos = TManCustomSkins.GetField("m_SkinInfos", BindingFlags.NonPublic | BindingFlags.Instance);
            var thing = (ManCustomSkins.CorporationSkins[])m_SkinInfos.GetValue(ManCustomSkins.inst);
            foreach (CustomSkin skin in CustomSkins)
            {
                try
                {
                    Console.WriteLine("- " + skin.corporationSkinInfo.m_SkinUIInfo.m_LocalisedString.m_Bank);
                    var Byte = (byte)thing[(int)skin.Faction].m_SkinsInCorp.Count;
                    SwapDictByteToID.Add(new BytePair() { ID = Byte, Faction = (byte)skin.Faction }, skin.ID);
                    SwapDictIDToByte.Add(skin.ID, new BytePair() { ID = Byte, Faction = (byte)skin.Faction });
                    thing[(int)skin.Faction].m_SkinsInCorp.Add(skin.corporationSkinInfo);
                }
                catch (Exception E)
                {
                    Console.WriteLine(E.ToString());
                }
            }
            Console.WriteLine("TextureSwapper: Done!");
            TManCustomSkins.GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ManCustomSkins.inst, null);
        }
    }

    class CustomSkin
    {
        public CorporationSkinInfo corporationSkinInfo;
        public string ID;
        public FactionSubTypes Faction;

        static Sprite SpriteFromImage(Texture2D texture, float Scale = 1f) => Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width * 0.5f, texture.height * 0.5f), Mathf.Max(texture.width, texture.height) * Scale);

        public CustomSkin(string Name, string ID, FactionSubTypes Faction, Texture2D Albedo, Texture2D Metallic, Texture2D Emissive, Texture2D Preview, Texture2D Button, Texture2D ButtonMini)
        {
            this.ID = ID;
            this.Faction = Faction;
            var preview = Preview != null ? SpriteFromImage(Preview) : SpriteFromImage(Albedo);
            var button = Button != null ? SpriteFromImage(Button) : preview;
            corporationSkinInfo = ScriptableObject.CreateInstance<CorporationSkinInfo>();
            corporationSkinInfo.m_SkinTextureInfo = new SkinTextures()
            {
                m_Albedo = Albedo,
                m_Metal = Metallic,
                m_Emissive = Emissive
            };
            Console.WriteLine("SetTextures");
            corporationSkinInfo.m_SkinUIInfo = new CorporationSkinUIInfo()
            {
                m_LocalisedString = new LocalisedString()
                {
                    m_Bank = Name
                },
                m_PreviewImage = preview,
                m_SkinButtonImage = button,
                m_SkinMiniPaletteImage = ButtonMini != null ? SpriteFromImage(ButtonMini) : button,
                m_SkinLocked = false
            };
        }
    }
}
