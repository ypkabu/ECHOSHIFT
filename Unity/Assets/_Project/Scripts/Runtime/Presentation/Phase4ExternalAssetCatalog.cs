using UnityEngine;

namespace EchoShift.Presentation
{
    [CreateAssetMenu(menuName = "ECHO SHIFT/Phase 4 External Asset Catalog")]
    public sealed class Phase4ExternalAssetCatalog : ScriptableObject
    {
        [SerializeField] private GameObject[] floorModules = System.Array.Empty<GameObject>();
        [SerializeField] private GameObject[] wallModules = System.Array.Empty<GameObject>();
        [SerializeField] private GameObject[] columnModules = System.Array.Empty<GameObject>();
        [SerializeField] private GameObject[] propModules = System.Array.Empty<GameObject>();
        [SerializeField] private GameObject doorFrame;
        [SerializeField] private GameObject doorPanel;
        [SerializeField] private GameObject robotVisual;
        [SerializeField] private Material floorMaterial;
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material darkMaterial;
        [SerializeField] private Material trimMaterial;

        public GameObject[] FloorModules => floorModules;
        public GameObject[] WallModules => wallModules;
        public GameObject[] ColumnModules => columnModules;
        public GameObject[] PropModules => propModules;
        public GameObject DoorFrame => doorFrame;
        public GameObject DoorPanel => doorPanel;
        public GameObject RobotVisual => robotVisual;
        public Material FloorMaterial => floorMaterial;
        public Material WallMaterial => wallMaterial;
        public Material DarkMaterial => darkMaterial;
        public Material TrimMaterial => trimMaterial;

        public bool IsComplete =>
            HasAll(floorModules, 4) && HasAll(wallModules, 6) &&
            HasAll(columnModules, 3) && HasAll(propModules, 7) &&
            doorFrame != null && doorPanel != null && robotVisual != null &&
            floorMaterial != null && wallMaterial != null &&
            darkMaterial != null && trimMaterial != null;

        public void Configure(
            GameObject[] floors,
            GameObject[] walls,
            GameObject[] columns,
            GameObject[] props,
            GameObject frame,
            GameObject panel,
            GameObject robot,
            Material floor,
            Material wall,
            Material dark,
            Material trim)
        {
            floorModules = floors ?? System.Array.Empty<GameObject>();
            wallModules = walls ?? System.Array.Empty<GameObject>();
            columnModules = columns ?? System.Array.Empty<GameObject>();
            propModules = props ?? System.Array.Empty<GameObject>();
            doorFrame = frame;
            doorPanel = panel;
            robotVisual = robot;
            floorMaterial = floor;
            wallMaterial = wall;
            darkMaterial = dark;
            trimMaterial = trim;
        }

        private static bool HasAll(GameObject[] values, int minimum)
        {
            if (values == null || values.Length < minimum) return false;
            for (int i = 0; i < values.Length; i++)
                if (values[i] == null) return false;
            return true;
        }
    }
}
