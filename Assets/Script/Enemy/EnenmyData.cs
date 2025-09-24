
using UnityEngine;

[CreateAssetMenu(fileName = "New Enenmy Data", menuName = "Game/Enemy Data")]
    public class EnemyData:ScriptableObject
    {
        [Header("属性")]
        [SerializeField]
        private int id;

        [SerializeField]
        private string name;

        [SerializeField]
        private float speed;

        [SerializeField]
        private float attack;
        
        [SerializeField]
        private float health;

        [Header("图标")]
        [SerializeField]
        private Sprite icon;

        public int ID
        {
            get => id;
            set => id = value;
        }

        public string Name
        {
            get => name;
            set => name = value;
        }

        public float Speed
        {
            get => speed;
            set => speed = value;
        }

        public float Attack
        {
            get => attack;
            set => attack = value;
        }

        public float Health
        {
            get => health;
            set => health = value;
        }

        public Sprite Icon
        {
            get => icon;
            set => icon = value;
        }
    }
