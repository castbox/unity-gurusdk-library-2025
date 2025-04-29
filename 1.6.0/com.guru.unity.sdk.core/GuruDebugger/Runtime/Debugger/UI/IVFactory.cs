

namespace Guru
{
    using System;
    using UnityEngine;
    
    
    public interface IViewFactory
    {
        UITabItem BuildTab(string tabName);
        UIOptionItem BuildOption(string optName);
    }
    
    public interface IWidgetFactory 
    {

        VButton BuildButton(string name, Action onClick, Transform parent);
        VLabel BuildLabel(string lbName, TextAnchor align, Transform parent);
        
    }
}