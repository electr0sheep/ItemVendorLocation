using ImGuiScene;
using System;

namespace UIDev.Framework
{
    internal interface IPluginUIMock : IDisposable
    {
        void Initialize(SimpleImGuiScene scene);
    }
}