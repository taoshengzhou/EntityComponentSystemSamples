using UnityEngine;
using DotsUI.Core;
using Unity.Entities;

public class PrefabExample : MonoBehaviour
{

    [SerializeField] Canvas m_TopCanvas;
    [SerializeField] Canvas m_RightCanvas;
    [SerializeField] Canvas m_PrefabDestination;

    // Start is called before the first frame update
    void Start()
    {
        if (World.DefaultGameObjectInjectionWorld == null)
            DefaultWorldInitialization.Initialize("UI World", false);
        //var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<UserInputSystemGroup>().AddSystemToUpdateList(World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<InstantiationSystem>());
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<UserInputSystemGroup>().AddSystemToUpdateList(World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CloseButtonSystem>());

        //RectTransformToEntity transformToEntity = new RectTransformToEntity(100, Allocator.Temp);
        //RectTransformConversionUtils.ConvertCanvasHierarchy(m_TopCanvas, World.DefaultGameObjectInjectionWorld.EntityManager, transformToEntity);
        //RectTransformConversionUtils.ConvertCanvasHierarchy(m_RightCanvas, World.DefaultGameObjectInjectionWorld.EntityManager, transformToEntity);
        //RectTransformConversionUtils.ConvertCanvasHierarchy(m_PrefabDestination, World.DefaultGameObjectInjectionWorld.EntityManager, transformToEntity);

        //GameObject.Destroy(m_TopCanvas.gameObject);
        //GameObject.Destroy(m_RightCanvas.gameObject);
        //GameObject.Destroy(m_PrefabDestination.gameObject);
        //transformToEntity.Dispose();
    }
}
