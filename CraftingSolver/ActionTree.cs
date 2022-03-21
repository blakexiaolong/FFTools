using System.Collections.Generic;

namespace CraftingSolver
{
    internal class ActionTree
    {
        Dictionary<int, ActionNode> Children { get; set; }

        public ActionTree()
        {
            Children = new Dictionary<int, ActionNode>();
        }
        public bool Add(List<Action> actions)
        {
            ActionNode head = null;
            for (int i = 0; i < actions.Count; i++)
            {
                if (i == 0)
                {
                    if (Children.ContainsKey(actions[i].ID))
                    {
                        if (Children[actions[i].ID].Failed) return false;
                    }
                    else
                    {
                        Children.Add(actions[i].ID, new ActionNode(actions[i], head));
                    }
                    head = Children[actions[i].ID];
                }
                if (i == actions.Count - 1)
                {
                    if (head.Children.ContainsKey(actions[i].ID))
                    {
                        return false;
                    }
                    else
                    {
                        head.Children.Add(actions[i].ID, new ActionNode(actions[i], head));
                    }
                }
                else
                {
                    if (head.Children.ContainsKey(actions[i].ID))
                    {
                        if (Children[actions[i].ID].Failed) return false;
                    }
                    else
                    {
                        head.Children.Add(actions[i].ID, new ActionNode(actions[i], head));
                    }
                    head = head.Children[actions[i].ID];
                }
            }
            return true;
        }
    }
    internal class ActionNode
    {
        public Action Action { get; set; }
        public Dictionary<int, ActionNode> Children { get; set; }
        public ActionNode Parent { get; set; }
        public bool Failed { get; set; }

        public ActionNode(Action action, ActionNode parent)
        {
            Action = action;
            Children = new Dictionary<int, ActionNode>();
            Parent = parent;
            Failed = false;
        }
        public List<Action> GetPath()
        {
            List<Action> path = new List<Action>() { Action };
            ActionNode head = Parent;
            while (head.Parent != null)
            {
                path.Insert(0, head.Action);
                head = head.Parent;
            }
            return path;
        }
    }
}
