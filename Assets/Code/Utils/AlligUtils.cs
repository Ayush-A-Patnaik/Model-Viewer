using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
//using AYellowpaper.SerializedCollections;
using System.Collections;

namespace AlligUtils
{
    public enum Palette
    {
        BlackAsNight,
        WhiteAsMilk,
        RedLikeBlood,
        GreenLikeGrass,
        BlueAsSky,
        YellowSunflower,
        CYANideAndHappiness,
        PuprleAsGrapes,
        SilverLikeTargaryen,
        OffWhite,
        Maroon,
        RetardedGreen,
        Violet,
        DecentBlue,
        Ocean,
        Orange,
        PinkLikeeeee,
        JustBrown,
        GoldenHours,
        BoringBeige
    }
    public static class Extensions
    {
        public static void RunAfterDelay(MonoBehaviour owner, System.Action action, float delay = .1f)
        {
            owner.StartCoroutine(WaitLayout(action, delay));
        }

        private static IEnumerator WaitLayout(System.Action action, float delay)
        {
            Canvas.ForceUpdateCanvases();
            yield return new WaitForSecondsRealtime(delay);
            Canvas.ForceUpdateCanvases();
            action?.Invoke();
        }
        public static Dictionary<(string, Palette), string> Colors =
        new()
        {
            { ("black", Palette.BlackAsNight), "#000000" },
            { ("white", Palette.WhiteAsMilk), "#FFFFFF" },
            { ("red", Palette.RedLikeBlood), "#FF0000" },
            { ("green", Palette.GreenLikeGrass), "#00FF00" },
            { ("blue", Palette.BlueAsSky), "#0000FF" },
            { ("yellow", Palette.YellowSunflower), "#FFFF00" },
            { ("cyan", Palette.CYANideAndHappiness), "#00FFFF" },
            { ("magenta", Palette.PuprleAsGrapes), "#FF00FF" },
            { ("silver", Palette.SilverLikeTargaryen), "#C0C0C0" },
            { ("gray", Palette.OffWhite), "#808080" },
            { ("maroon", Palette.Maroon), "#800000" },
            { ("olive", Palette.RetardedGreen), "#808000" },
            { ("purple", Palette.Violet), "#800080" },
            { ("teal", Palette.DecentBlue), "#008080" },
            { ("navy", Palette.Ocean), "#000080" },
            { ("orange", Palette.Orange), "#FFA500" },
            { ("pink", Palette.PinkLikeeeee), "#FFC0CB" },
            { ("brown", Palette.JustBrown), "#A52A2A" },
            { ("gold", Palette.GoldenHours), "#FFD700" },
            { ("beige", Palette.BoringBeige), "#F5F5DC" }
        };

        /// <summary>
        /// Returns Child with given Tag
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static Transform GetChildWithTag(this Transform parent, string tag)
        {
            foreach (Transform child in parent)
            {
                if (child.CompareTag(tag))
                {
                    return child;
                }

                // Recursively search in child's children
                Transform childResult = child.GetChildWithTag(tag);
                if (childResult != null)
                {
                    return childResult;
                }
            }

            return null;
        }

        /// <summary>
        /// Retruns child with given name
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Transform GetChildWithName(this Transform parent, string name)
        {
            Transform target = null;
            if (parent.name == name) return target;
            foreach (Transform child in parent)
            {
                if (child.name == name)
                {
                    return child;
                }
                Transform result = GetChildWithName(child, name);
                if (result != null) return result;
            }
            return target;
        }

        public static string Multiply(this string source, int multiplier)
        {
            StringBuilder sb = new StringBuilder(multiplier * source.Length);
            if (multiplier == 0)
                return source;
            for (int i = 0; i < multiplier; i++)
            {
                sb.Append(source);
            }

            return sb.ToString();
        }

        public static List<GameObject> GetChildrenWithName(this Transform parent, string name)
        {
            List<GameObject> childrenWithName = new();
            foreach (Transform child in parent)
            {
                if (child.name == name)
                {
                    childrenWithName.Add(child.gameObject);
                }
                List<GameObject> childrenWithSameName = child.GetChildrenWithName(name);
                childrenWithName.AddRange(childrenWithSameName);
            }
            return childrenWithName;
        }

        public static string ToExplorerPath(this string str)
        {
            return str.Replace('/', '\\');
        }
        public static string ToUnityPath(this string str)
        {
            return str.Replace('\\', '/');
        }

        private static string GetColorHex(object color)
        {
            if (color is Palette palleteColor)
            {
                var entry = Colors.FirstOrDefault(c => c.Key.Item2 == palleteColor);
                return entry.Value ?? "#808080";
            }
            else if (color is string colorString)
            {
                var entry = Colors.FirstOrDefault(c => c.Key.Item1.Equals(colorString, StringComparison.OrdinalIgnoreCase));
                return entry.Value ?? "#808080";
            }
            return "#808080";
        }

        public static void Print<T>(this T value, string message, object color)
        {
            var hex = GetColorHex(color);
            //Debug.Log(message + value);
            Debug.Log($"<color={hex}><i>{message}{value}</i></color>");
        }

        /// <summary>
        /// will print in yellow by default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="message"></param>
        public static void Print<T>(this T value, string message = "")
        {
            var hex = GetColorHex(Palette.YellowSunflower);
            Debug.Log($"<color={hex}><i>{message}{value}</i></color>");
        }

        public static void PrintError<T>(this T value, string message, object color)
        {
            var hex = GetColorHex(color);
            Debug.LogError($"<color={hex}><i>{message}{value}</i></color>");
        }

        public static void PrintError<T>(this T value, string message = "")
        {
            var hex = GetColorHex(Palette.RedLikeBlood);
            Debug.LogError($"<color={hex}><i>{message}{value}</i></color>");
        }

        public static void Print<T>(this List<T> list, string message, object color)
        {
            var hex = GetColorHex(color);
            for (int i = 0; i < list.Count; i++)
            {
                //Debug.Log(message + list[i]);
                Debug.Log($"<color={hex}><b>{message}{list[i]}</b></color>");
            }
        }
        public static void Print<T>(this HashSet<T> list, string message, object color)
        {
            var hex = GetColorHex(color);
            for (int i = 0; i < list.Count; i++)
            {
                //Debug.Log(message + list.ElementAt(i));
                Debug.Log($"<color={hex}><b>{message}{list.ElementAt(i)}</b></color>");
            }
        }

        /// <summary>
        /// Returns a colored string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string ColorThis<T>(this T value, object color)
        {
            var hex = GetColorHex(color);
            string str = $"<color={hex}>{value}</color>";
            return value.ToString();
        }

        public static int ToInt(this string str)
        {
            if (int.TryParse(str, out int result)) return result;
            else
            {
                str.Print("Couldnt convert string to int: ", Palette.OffWhite); return -1;
            }
        }

        public static float ToFloat(this string str)
        {
            if (float.TryParse(str, out float result)) return result;
            else
            {
                str.Print("Couldnt convert string to float: ", Palette.OffWhite); return -1;
            }
        }

        public static bool HasComponent<T>(this GameObject flag) where T : Component
        {
            return flag.GetComponent<T>() != null;
        }
    }

    public class ValuePair<T1, T2>
    {
        public T1 Val1 { get; set; }
        public T2 Val2 { get; set; }

        public ValuePair(T1 v1, T2 v2)
        {
            Val1 = v1;
            Val2 = v2;
        }
    }

    // [System.Serializable]
    // public class Triplet<TKey, T1, T2>
    // {
    //     public SerializedDictionary<TKey, ValuePair<T1, T2>> m_Triplet;
    //
    //     public Triplet()
    //     {
    //         // _triplet = new Dictionary<TKey, ValuePair<T1, T2>>();
    //         m_Triplet = new SerializedDictionary<TKey, ValuePair<T1, T2>>();
    //     }
    //
    //     public void Add(TKey key, T1 v1, T2 v2)
    //     {
    //         m_Triplet[key] = new ValuePair<T1, T2>(v1, v2);
    //     }
    //
    //     public void Remove(TKey key)
    //     {
    //         m_Triplet.Remove(key);
    //     }
    //
    //     public bool ContainsKey(TKey key)
    //     {
    //         return m_Triplet.ContainsKey(key);
    //     }
    //
    //     public object Get(TKey key, int index)
    //     {
    //         if (m_Triplet.TryGetValue(key, out ValuePair<T1, T2> pair))
    //         {
    //             if (index == 0)
    //                 return pair.Val1;
    //             else if (index == 1)
    //                 return pair.Val2;
    //             else
    //                 throw new ArgumentOutOfRangeException(nameof(index), "Index must be 0 or 1");
    //         }
    //         throw new KeyNotFoundException($"The key '{key}' was not found in the dictionary.");
    //     }
    //
    //     public bool TryGetValue(TKey key, out T1 value1, out T2 value2)
    //     {
    //         if (m_Triplet.TryGetValue(key, out ValuePair<T1, T2> pair))
    //         {
    //             value1 = pair.Val1;
    //             value2 = pair.Val2;
    //             return true;
    //         }
    //         value1 = default(T1);
    //         value2 = default(T2);
    //         return false;
    //     }
    //
    //
    //
    // }


}




public struct HexColor
{
    public string Hex;

    public HexColor(string hex)
    {
        Hex = hex;
    }

    public static implicit operator Color(HexColor hexColor)
    {
        return hexColor.ToColor();
    }
    public static implicit operator Color32(HexColor hexColor)
    {
        return hexColor.ToColor();
    }

    public Color ToColor()
    {
        Color color;
        ColorUtility.TryParseHtmlString(Hex, out color);
        return color;
    }

}


