using UnityEngine;


// using UnityEngine.UI;
// using System.Reflection;


namespace Misc {
    public static class Extensions {
        public static Vector2Int ToVector2Int (this Vector3Int v) {
            return new Vector2Int(v.x, v.y);
        }

        public static Vector2Int ToVector2Int (this Vector3 v) {
            return new Vector2Int((int)v.x, (int)v.y);
        }

        public static Vector3Int ToVector3Int (this Vector2Int v) {
            return new Vector3Int(v.x, v.y, 0);
        }

        public static Vector3Int ToVector3Int (this Vector3 v) {
            return new Vector3Int((int)v.x, (int)v.y, (int)v.z);
        }

        public static Vector3Int Modified (this Vector3Int v, int? x = null, int? y = null, int? z = null) {
            return new Vector3Int(
                x != null ? (int) (v.x + x) : v.x,
                y != null ? (int) (v.y + y) : v.y,
                z != null ? (int) (v.z + z) : v.z
            );
        }
    }
}


// public static class UISetExtensions {
//     static MethodInfo toggleSetMethod;
//
//     static UISetExtensions () {
//         MethodInfo[] methods = typeof(Toggle).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
//         for (var i = 0; i < methods.Length; i++) {
//             if (methods[i].Name == "Set" && methods[i].GetParameters().Length == 2) {
//                 toggleSetMethod = methods[i];
//                 break;
//             }
//         }
//     }
//
//     public static void Set (this Toggle instance, bool value, bool sendCallback) {
//         toggleSetMethod.Invoke(instance, new object[] {value, sendCallback});
//     }
// }
