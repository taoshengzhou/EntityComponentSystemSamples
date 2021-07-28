using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace DotsUI.Core
{
    [UpdateInGroup(typeof(ElementMeshUpdateSystemGroup))]
    class UpdateElementColorSystem : JobComponentSystem
    {
        private EntityQuery m_ColorUpdateQuery;
        protected override void OnCreate()
        {
            m_ColorUpdateQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadOnly<UpdateElementColor>(),
                    ComponentType.ReadOnly<VertexColorValue>(),
                    ComponentType.ReadOnly<VertexColorMultiplier>(),
                    ComponentType.ReadWrite<ControlVertexData>(),
                    ComponentType.ReadOnly<RebuildElementMeshFlag>(), 
                }
            });
        }

        [BurstCompile]
        struct UpdateColorVertices : IJobChunk
        {
            [NativeDisableContainerSafetyRestriction] public BufferFromEntity<MeshVertex> VertexFromCanvasEntity;
            [ReadOnly] public ComponentDataFromEntity<Parent> ParentFromEntity;
            [NativeDisableContainerSafetyRestriction]public BufferTypeHandle<ControlVertexData> VertexDataType;
            [ReadOnly] public ComponentTypeHandle<ElementVertexPointerInMesh> VertexPointerInCanvasMeshType;
            [ReadOnly] public EntityTypeHandle EntityType;
            [ReadOnly] public ComponentTypeHandle<VertexColorValue> VertexColorType;
            [ReadOnly] public ComponentTypeHandle<VertexColorMultiplier> VertexColorMultiplierType;
            [ReadOnly] public ComponentTypeHandle<RebuildElementMeshFlag> RebuildElementMeshFlagType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var colorArray = chunk.GetNativeArray(VertexColorType);
                var colorMultiplierArray = chunk.GetNativeArray(VertexColorMultiplierType);
                var entityArray = chunk.GetNativeArray(EntityType);
                var vertexDataAccessor = chunk.GetBufferAccessor(VertexDataType);
                var pointerArray = chunk.GetNativeArray(VertexPointerInCanvasMeshType);
                var rebuildMeshFlagArray = chunk.GetNativeArray(RebuildElementMeshFlagType);

                for (int i = 0; i < chunk.Count; i++)
                {
                    if(!rebuildMeshFlagArray[i].Rebuild)
                    {
                        var controlVertexBuffer = vertexDataAccessor[i];
                        int bufferLen = controlVertexBuffer.Length;
                        var color = colorArray[i].Value* colorMultiplierArray[i].Value;

                        Entity root = GetRootRecursive(entityArray[i]);
                        var canvasVertexBuffer = VertexFromCanvasEntity[root];
                        int pointerToCanvasVertex = pointerArray[i].VertexPointer;

                        for (int j = 0; j < bufferLen; j++)
                        {
                            var controlVertex = controlVertexBuffer[j];
                            controlVertex.Color = color;
                            controlVertexBuffer[j] = controlVertex;
                            canvasVertexBuffer[j + pointerToCanvasVertex] = controlVertex;
                        }
                    }
                }
            }

            public Entity GetRootRecursive(Entity entity)
            {
                if (ParentFromEntity.HasComponent(entity))
                    return GetRootRecursive(ParentFromEntity[entity].Value);
                return entity;
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            UpdateColorVertices updateJob = new UpdateColorVertices()
            {
                EntityType = GetEntityTypeHandle(),
                ParentFromEntity = GetComponentDataFromEntity<Parent>(true),
                VertexColorType = GetComponentTypeHandle<VertexColorValue>(true),
                VertexColorMultiplierType = GetComponentTypeHandle<VertexColorMultiplier>(true),
                VertexDataType = GetBufferTypeHandle<ControlVertexData>(),
                VertexFromCanvasEntity = GetBufferFromEntity<MeshVertex>(),
                VertexPointerInCanvasMeshType = GetComponentTypeHandle<ElementVertexPointerInMesh>(true),
                RebuildElementMeshFlagType = GetComponentTypeHandle<RebuildElementMeshFlag>(true)
            };
            inputDeps = updateJob.Schedule(m_ColorUpdateQuery, inputDeps);
            return inputDeps;
        }
    }
}
