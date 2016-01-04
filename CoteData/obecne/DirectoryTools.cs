using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Utils
{
    public class DataDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private List<TValue> m_data = new List<TValue>();
        private List<TKey> m_keys = new List<TKey>();

        public void Add(TKey key, TValue value)
        {
            try
            {
                base.Add(key, value);
                m_data.Add(value);
                m_keys.Add(key);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Ex: " + e.Message);
            }
        }

        public void AddUpdate(TKey key, TValue value)
        {
            try
            {
                if (base.ContainsKey(key))
                {
                    base[key] = value;
                }
                else
                {
                    base.Add(key, value);
                    m_data.Add(value);
                    m_keys.Add(key);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Ex: " + e.Message);
            }
        }

        public new void Clear()
        {
            base.Clear();
            m_data.Clear();
            m_keys.Clear();
        }

        public TValue this[int index]
        {
            get
            {
                return m_data[index];
            }
            set
            {
                m_data[index] = value;
            }
        }

        public TValue Get(int index)
        {
            return m_data[index];
        }

        public TKey GetKey(int index)
        {
            return m_keys[index];
        }

        public TValue GetValue(TKey key)
        {
            if (base.ContainsKey(key) == true)
                return base[key];
            else
                return default(TValue);
        }

        public void Sort(IComparer<TValue> comparer)
        {
            m_data.Sort(comparer);
        }


        public int IndexOf(TValue item)
        {
            return m_data.IndexOf(item);
        }


        public bool Remove(TKey key)
        {
            bool ret = false;
            if (base.ContainsKey(key) == true)
            {

                TValue val = base[key];
                ret = m_data.Remove(val);
                ret = base.Remove(key);
            }
            return ret;
        }

        public TValue[] ToArray()
        {
            return m_data.ToArray();
        }
    }

    public class KeyValueList<T1, T2>
    {
        List<KeyValuePair<T1, T2>> m_List = new List<KeyValuePair<T1, T2>>();
        public void Add(T1 t1, T2 t2)
        {
            KeyValuePair<T1, T2> pair = new KeyValuePair<T1, T2>(t1, t2);
            m_List.Add(pair);

        }

        public T1 GetKey(int idx)
        {
            return m_List[idx].Key;
        }

        public T2 GetValue(int idx)
        {
            return m_List[idx].Value;
        }

        public void Clear()
        {
            m_List.Clear();
        }

        public void RemoveAt(int idx)
        {
            m_List.RemoveAt(idx);
        }

        public int Count
        {
            get
            {
                return m_List.Count;
            }
        }
    }

    /// <summary>
    /// Trida, ktera vybere ye dvou seznamu spolecne prvky
    /// Trida, ktera vybere ye dvou seznamu rozdilne prvky
    /// </summary>
    public class GSIntersectionList<T>
    {
        List<T> m_lista = null;
        List<T> m_listb = null;

        public GSIntersectionList(List<T> lista, List<T> listb)
        {
            m_lista = lista;
            m_listb = listb;
        }

        /// <summary>
        /// Vrat prunik
        /// </summary>
        /// <returns></returns>
        public List<T> getIntersection()
        {
            List<T> inter = new List<T>();

            for (int i = 0; i < m_lista.Count; i++)
            {
                if (m_listb.IndexOf(m_lista[i]) != -1)
                {
                    inter.Add(m_lista[i]);
                }
            }

            return inter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<T> getComplement()
        {
            List<T> comp = new List<T>();

            for (int i = 0; i < m_lista.Count; i++)
            {
                if (m_listb.IndexOf(m_lista[i]) == -1)
                {
                    comp.Add(m_lista[i]);
                }
            }
            for (int i = 0; i < m_listb.Count; i++)
            {
                if (m_lista.IndexOf(m_listb[i]) == -1)
                {
                    comp.Add(m_listb[i]);
                }
            }

            return comp;
        }
    }

    /// <summary>
    /// Seznamy typu - slouzi k dohledavani podle string a indexu
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GSTypeList<T>
    {
        private Dictionary<string, ArrayList> m_dict = new Dictionary<string, ArrayList>();
        private Dictionary<int, string> m_index = new Dictionary<int, string>();
        private List<T> m_list = new List<T>();

        public void Add(string key, T value)
        {
            if (m_dict.ContainsKey(key) == false)
            {
                ArrayList seznam = new ArrayList();
                seznam.Add(value);
                m_dict.Add(key, seznam);
            }
            else
            {
                ArrayList seznam = m_dict[key];
                seznam.Add(value);
            }
            m_list.Add(value);
            m_index.Add(m_list.Count - 1, key);
        }

        /// <summary>
        /// Vraci podle dvou parametru ... key, index
        /// pokud neni polozka v seznamu vraci null
        /// </summary>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public T Get(string key, int index)
        {
            if (m_dict.ContainsKey(key) == true)
            {
                ArrayList seznam = m_dict[key];
                if (index < seznam.Count)
                {
                    return (T)seznam[index];
                }
                else
                {
                    return default(T);
                }
            }

            return default(T);
        }

        /// <summary>
        /// vraci objekt 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T Get(int index)
        {

            return (T)m_list[index];
        }

        public int GetTypeIndex(int index, ref string typ)
        {
            T type_obj = m_list[index];
            string key = m_index[index];
            typ = key;
            ArrayList pole = m_dict[key];

            return pole.IndexOf(type_obj);
        }

        public void Clear()
        {
            m_dict.Clear();
        }

        public int Count
        {
            get
            {
                return m_list.Count;
            }
        }
    }
}