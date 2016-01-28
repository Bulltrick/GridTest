using UnityEngine;
using System.Collections;
using System;

namespace GridTest
{

    public class CellScript : MonoBehaviour
    {
        public GameObject GameLogic;
        public GameObject PathMarker; // Prefab for path marker
        private GameObject PathMarked; // gos with which the path is marked
        private GameObject EndMarked; // go with which the end of the path is marked
        public int Height;
        private Color c;

        // Use this for initialization
        void Start()
        {
            //Set random Height
            /*
            Height = UnityEngine.Random.Range(0, 127);
            float f = 1-(Height / 127f);
            c = new Color(f, f, f);
            GetComponent<Renderer>().material.color = c;
            */
        }

        // Update is called once per frame
        void Update()
        {

        }

        // Called from GameLogic, when the cell is created
        public void SetHeight(int h)
        {
            Height = h;
            float f = 1 - (Height / 127f);
            c = new Color(f, f, f);
            GetComponent<Renderer>().material.color = c;
        }

        void OnMouseDown()
        {
            Vector3 vec = new Vector3(gameObject.transform.position.x + 0.5f, gameObject.transform.position.y + 0.5f, 1f);
            EndMarked = Instantiate(PathMarker, transform.position, transform.rotation) as GameObject;
            EndMarked.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            transform.parent.GetComponent<GameLogic>().newClick(gameObject);
        }

        internal int GetHeight()
        {
            return this.Height;
        }

        public void loseSelection()
        {
            Destroy(EndMarked);
            GetComponent<Renderer>().material.color = c;
        }

        public void isPath()
        {
            PathMarked = Instantiate(PathMarker, transform.position, transform.rotation) as GameObject;
        }

        internal void remmovePath()
        {
            Destroy(PathMarked);
        }
    }

}