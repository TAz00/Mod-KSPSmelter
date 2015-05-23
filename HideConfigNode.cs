using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSPSmelter
{
   
    public class HideConfigNode
    {
        private ConfigNode node = null;
        public HideConfigNode(ConfigNode node)
        {
            this.SetConfigNode(node);
        }
        public HideConfigNode()
        {
           
        }
        public void SetConfigNode(ConfigNode node)
        {
            this.node = node;
        }
        public void SetValue(string name, string value)
        {
            node.SetValue(name, value);
        }
        public ConfigNode GetConfigNode()
        {
            return this.node;
        }
    }

    public class HideConfigNodeArray
    {
        private ConfigNode[] node = null;
        public HideConfigNodeArray(ConfigNode[] nodes)
        {
            this.SetConfigNode(nodes);
        }
        public HideConfigNodeArray()
        {

        }
        public void SetConfigNode(ConfigNode[] nodes)
        {
            this.node = nodes;
        }

        public ConfigNode[] GetConfigNode()
        {
            return this.node;
        }
    }
}
