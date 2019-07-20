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
    public static class Main
    {
        internal static Dictionary<FactionSubTypes, int> CustomSkinCounter;
        internal static Dictionary<byte, Dictionary<byte, string>> SwapDictByteToID;
        internal static Dictionary<byte, Dictionary<string, byte>> SwapDictIDToByte;

        static List<CustomSkin> CustomSkins = new List<CustomSkin>();

        public static void Init()
        {
            var harmony = HarmonyInstance.Create("aceba1.textureswapper");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            LoadTextures();
            new GameObject().AddComponent<SkinReloader>();
        }

        private class SkinReloader : MonoBehaviour
        {
            void Update()
            {
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
                {
                    Console.WriteLine("TextureSwapper: Begin");
                    foreach (var skin in CustomSkins)
                    {
                        GetSkinData(skin.Path, skin);
                    }
                    ApplySkins(true);

                    var tMTMS = typeof(ManTechMaterialSwap);
                    var fOML = tMTMS.GetField("m_OriginalMaterialLookup", BindingFlags.NonPublic | BindingFlags.Instance);
                    var list = fOML.GetValue(ManTechMaterialSwap.inst) as Dictionary<string, int>;
                    list.Clear();
                    tMTMS.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(ManTechMaterialSwap.inst, null);
                }
            }
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

        static void Foo(string TexPath, string type)
        {
            string path = Path.Combine(TexPath, type.ToUpper() + ".ExamplePack.tsmod");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                var ndir = Directory.GetFiles(Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, "../example/" + type));
                Console.WriteLine("Created directory at " + path + ", getting " + ndir.Length + " files");
                foreach (string file in ndir)
                {
                    FileInfo F = new FileInfo(file);
                    File.Copy(file, Path.Combine(path, F.Name));
                }
            }
        }

        static void LoadTextures()
        {
            CustomSkinCounter = new Dictionary<FactionSubTypes, int>();
            SwapDictByteToID = new Dictionary<byte, Dictionary<byte, string>>();
            SwapDictIDToByte = new Dictionary<byte, Dictionary<string, byte>>();

            var dir = new DirectoryInfo(Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, "../../../"));
            string TexPath = Path.Combine(dir.FullName, "Custom Textures");
            try
            {
                if (!Directory.Exists(TexPath))
                {
                    Directory.CreateDirectory(TexPath);
                    Console.WriteLine("Created Custom Textures folder");
                }
                Foo(TexPath, "gso");
                Foo(TexPath, "ven");
                Foo(TexPath, "gc");
                Foo(TexPath, "he");
                Foo(TexPath, "bf");
            }
            catch (Exception E)
            {
                Console.WriteLine("Could not access \"" + TexPath + "\"!\n" + E.Message);
            }
            Console.WriteLine("TextureSwapper: Begin");
            foreach (var texpack in new DirectoryInfo(TexPath).EnumerateDirectories())
            {
                GetSkinData(texpack);
            }
            ApplySkins(false);
        }

        static void GetSkinData(DirectoryInfo texpack, CustomSkin Update = null)
        {
            try
            {
                if (!texpack.Exists) return;
                string Name = texpack.Name;
                int lastindex = Name.LastIndexOf('.');
                int firstindex = Name.IndexOf('.');
                Console.Write("\nReached " + texpack.Name);
                if (lastindex == -1 || firstindex == lastindex)
                {
                    Console.WriteLine("\n" + texpack.FullName + " does not have the proper naming scheme! (Folder must be named as Corp.Texture Name.Author, for example, GSO.Gold.1249 )");
                    return;
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
                if (!CustomSkinCounter.ContainsKey(Faction))
                {
                    CustomSkinCounter.Add(Faction, 0);
                }

                if (Update != null)
                {
                    Update.UpdateCorporationSkinInfo(Albedo, Metallic, Emission);
                    Update.UpdateSkinUIInfo(Preview, Button, ButtonMini);
                    Console.WriteLine("processing... \nUpdated skin in list!");
                }
                else
                {
                    CustomSkins.Add(new CustomSkin(texpack, Name, (byte)(255 - CustomSkinCounter[Faction]++), Faction, Albedo, Metallic, Emission, Preview, Button, ButtonMini));
                    Console.WriteLine("processing... \nAdded skin to list!");
                }
            }
            catch (Exception E)
            {
                Console.WriteLine("\nEXCEPTION! " + E.ToString());
            }
        }

        static void ApplySkins(bool Update)
        {
            Console.WriteLine("\nApplying skins...");
            Type TManCustomSkins = typeof(ManCustomSkins);
            var m_SkinInfos = TManCustomSkins.GetField("m_SkinInfos", BindingFlags.NonPublic | BindingFlags.Instance);
            var thing = (ManCustomSkins.CorporationSkins[])m_SkinInfos.GetValue(ManCustomSkins.inst);
            if (Update)
            {
                foreach (CustomSkin skin in CustomSkins)
                {
                    var t = thing[(int)skin.Faction].m_SkinsInCorp;
                    t.RemoveAt(t.Count-1);
                }
                foreach (CustomSkin skin in CustomSkins)
                {
                    try
                    {
                        Console.WriteLine("- " + skin.corporationSkinInfo.m_SkinUIInfo.m_LocalisedString.m_Bank);
                        thing[(int)skin.Faction].m_SkinsInCorp.Add(skin.corporationSkinInfo);
                    }
                    catch (Exception E)
                    {
                        Console.WriteLine(E.ToString());
                    }
                }
            }
            else
            {
                foreach (CustomSkin skin in CustomSkins)
                {
                    try
                    {
                        Console.WriteLine("- " + skin.corporationSkinInfo.m_SkinUIInfo.m_LocalisedString.m_Bank);
                        byte f = (byte)skin.Faction;
                        if (!SwapDictByteToID.ContainsKey(f))
                        {
                            SwapDictByteToID.Add(f, new Dictionary<byte, string>());
                            SwapDictIDToByte.Add(f, new Dictionary<string, byte>());
                        }
                        SwapDictByteToID[f].Add(skin.bID, skin.ID);
                        SwapDictIDToByte[f].Add(skin.ID, skin.bID);
                        thing[(int)skin.Faction].m_SkinsInCorp.Add(skin.corporationSkinInfo);
                    }
                    catch (Exception E)
                    {
                        Console.WriteLine(E.ToString());
                    }
                }
            }
            Console.WriteLine("TextureSwapper: Done!");
            try
            {
                TManCustomSkins.GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ManCustomSkins.inst, null);
            }
            catch (Exception E)
            {
                Console.WriteLine("TextureSwapper: Oop wait no, it is broken, hold on...");
                Console.WriteLine(E.ToString());
            }
        }
    }

    class CustomSkin
    {
        public CorporationSkinInfo corporationSkinInfo;
        public string ID;
        public byte bID;
        public DirectoryInfo Path;
        public FactionSubTypes Faction;

        static Sprite SpriteFromImage(Texture2D texture, float Scale = 1f) => Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(texture.width * 0.5f, texture.height * 0.5f), Mathf.Max(texture.width, texture.height) * Scale);

        public void UpdateCorporationSkinInfo(Texture2D Albedo = null, Texture2D Metallic = null, Texture2D Emissive = null)
        {
            var ti = corporationSkinInfo.m_SkinTextureInfo;
            if (Albedo != null) ti.m_Albedo = Albedo;
            if (Metallic != null) ti.m_Metal = Metallic;
            if (Emissive != null) ti.m_Emissive = Emissive;
        }

        public void UpdateSkinUIInfo(Texture2D Preview, Texture2D Button, Texture2D ButtonMini)
        {
            var suii = corporationSkinInfo.m_SkinUIInfo;

            if (Preview != null) suii.m_PreviewImage = SpriteFromImage(Preview);
            if (Button != null) suii.m_SkinButtonImage = SpriteFromImage(Button);
            if (ButtonMini != null) suii.m_SkinMiniPaletteImage = SpriteFromImage(ButtonMini);
        }

        public CustomSkin(DirectoryInfo Path, string Name, byte bID, FactionSubTypes Faction, Texture2D Albedo, Texture2D Metallic, Texture2D Emissive, Texture2D Preview, Texture2D Button, Texture2D ButtonMini)
        {
            this.Path = Path;
            ID = Name;
            this.bID = bID;
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
            Console.WriteLine(" >Setting Textures");
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
            try
            {
                UnstableWrap.AddMoreInfoToSkin(corporationSkinInfo, bID, Faction);
            }
            catch { /* fail silently */ }
        }
    }
}
