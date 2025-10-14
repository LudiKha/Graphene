using UnityEngine;

namespace Graphene
{
    /// <summary>
    /// Base class for all components that participate in the Graphene dependency injection system.
    /// Provides access to the injected <see cref="Graphene"/> instance and its <see cref="BindingsManager"/>.
    /// </summary>
    public class GrapheneComponent : MonoBehaviour, IGrapheneInjectable
    {
        /// <summary>
        /// Cached debug name for this component, used for identification and logging.
        /// </summary>
        internal string debugNameCached;

        /// <summary>
        /// Gets the cached debug name for this component.
        /// </summary>
        public string DebugName => debugNameCached;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.HideIf(nameof(graphene))]
#endif
        /// <summary>
        /// The injected Graphene instance that manages bindings and dependencies for this component.
        /// </summary>
        [SerializeField] protected Graphene graphene;

        /// <summary>
        /// Gets the injected <see cref="Graphene"/> instance.
        /// </summary>
        public Graphene Graphene => graphene;

        /// <summary>
        /// Gets the <see cref="BindingsManager"/> from the injected <see cref="Graphene"/> instance.
        /// </summary>
        public BindingsManager BindingsManager => graphene?.Binder;

        /// <summary>
        /// Injects the specified <see cref="Graphene"/> instance into this component.
        /// </summary>
        /// <param name="graphene">The Graphene instance to inject.</param>
        public virtual void Inject(Graphene graphene)
        {
            this.graphene = graphene;

            if (string.IsNullOrEmpty(debugNameCached))
                debugNameCached = name;
        }

        /// <summary>
        /// Unity Awake callback. Ensures the debug name is initialized.
        /// </summary>
        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(debugNameCached))
                debugNameCached = name;
        }
    }
}