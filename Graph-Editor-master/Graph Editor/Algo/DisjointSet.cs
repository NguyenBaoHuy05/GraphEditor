using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph_Editor
{
    class DisjointSet
    {
        public int[] rank, parent;
        public DisjointSet(int n)
        {
            rank = new int[n+1];
            parent = new int[n+1];
            for(int i = 0; i <= n; ++i)
            {
                parent[i] = i;
            }
        } 
        public int FindUParent(int node)
        {
            if (node == parent[node]) return node;
            return parent[node] = FindUParent(parent[node]);
        }
        public void UnionByRank(int u, int v)
        {
            int ulp_u = FindUParent(u);
            int ulp_v = FindUParent(v);
            if (ulp_u == ulp_v) return;
            if(rank[ulp_u] > rank[ulp_v])
            {
                parent[ulp_v] = ulp_u;
            }
            else if(rank[ulp_u] < rank[ulp_v]) 
            {
                parent[ulp_u] = ulp_v;
            }
            else
            {
                parent[ulp_u] = ulp_v;
                ++rank[ulp_v];
            }
        }
            
    }
}
