using System.Reflection;

namespace CMod {
    class Utils {
        /// <summary>
        /// Returns path to the directory from where current dll (the mod dll) is executed from.
        /// </summary>
        /// <returns></returns>
        public static string GetPathToCModDirectory() {
            // this returns path to the directory, desipite the function saying "Name"
            string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if(path == null) {
                throw new NullReferenceException("failed to get the current assembly");
            }

            return path;
        }

        /// <summary>
        /// Get path to the current mod root directory.
        /// </summary>
        /// <returns></returns>
        public static string GetPathToModRoot() {
            return Path.Combine(GetPathToCModDirectory(), "..");
        }
    }
}
