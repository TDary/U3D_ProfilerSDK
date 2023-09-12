[ProtoBuf.ProtoContract]
public struct SimpleData
{
    [ProtoBuf.ProtoMember(1)]
    public int frame { get; set; }  //片段帧数
    [ProtoBuf.ProtoMember(2)]
    public float fps { get; set; }
    [ProtoBuf.ProtoMember(3)]
    public float cpuTTime { get; set; }  //CPUTotalTime
    [ProtoBuf.ProtoMember(4)]
    public float renTime { get; set; }  //renderingTime
    [ProtoBuf.ProtoMember(5)]
    public float scrisTime { get; set; } //ScritsTime
    [ProtoBuf.ProtoMember(6)]
    public float pyhsTime { get; set; } //PhysicsTime
    [ProtoBuf.ProtoMember(7)]
    public float otherTime { get; set; } //OtherTime
    [ProtoBuf.ProtoMember(8)]
    public float gcTime{ get; set; }  //GCTime
    [ProtoBuf.ProtoMember(9)]
    public float uiTime { get; set; } //UITime
    [ProtoBuf.ProtoMember(10)]
    public float vncTime { get; set; }  //SyncVNCTime
    [ProtoBuf.ProtoMember(11)]
    public float giTime { get; set; } //GI Time
    [ProtoBuf.ProtoMember(12)]
    public float aniTIme{ get; set; }  //animationTime
    [ProtoBuf.ProtoMember(13)]
    public float setpassCall { get; set; }  //SetPassCallCOunt
    [ProtoBuf.ProtoMember(14)]
    public float drawCall { get; set; }  //DrawCallCOunt
    [ProtoBuf.ProtoMember(15)]
    public float batchs { get; set; }  //BatchCount
    [ProtoBuf.ProtoMember(16)]
    public float tris { get; set; }  //trianles
    [ProtoBuf.ProtoMember(17)]
    public float vertex { get; set; }  //vertex
    [ProtoBuf.ProtoMember(18)]
    public float dynamicDrawCall { get; set; }//dynamicBtachDrawCalls
    [ProtoBuf.ProtoMember(19)]
    public float dynamicCount { get; set; } //dynamicBtachCount
    [ProtoBuf.ProtoMember(20)]
    public float dynamicTrisCount { get; set; }  //dynamicBatchTrisCount
    [ProtoBuf.ProtoMember(21)]
    public float dynamicVertsCount { get; set; } //dynamicBatchVertexsCount
    [ProtoBuf.ProtoMember(22)]
    public float dynamicBatchTime { get; set; }  //dynamicBatchTime
    [ProtoBuf.ProtoMember(23)]
    public float staticDrawCall { get; set; }//staticBtachDrawCalls
    [ProtoBuf.ProtoMember(24)]
    public float staticCount { get; set; } //staticBtachCount
    [ProtoBuf.ProtoMember(25)]
    public float staticTrisCount { get; set; }  //staticBatchTrisCount
    [ProtoBuf.ProtoMember(26)]
    public float staticVertsCount { get; set; } //staticBatchVertexsCount
    [ProtoBuf.ProtoMember(27)]
    public float instanceDrawCall { get; set; }//instanceBtachDrawCalls
    [ProtoBuf.ProtoMember(28)]
    public float instanceCount { get; set; } //instanceBtachCount
    [ProtoBuf.ProtoMember(29)]
    public float instanceTrisCount { get; set; }  //instanceBatchTrisCount
    [ProtoBuf.ProtoMember(30)]
    public float instanceVertsCount { get; set; } //instanceBatchVertexsCount
    [ProtoBuf.ProtoMember(31)]
    public float usedTextureCount { get; set; }
    [ProtoBuf.ProtoMember(32)]
    public float usedTextureMemory { get; set; }
    [ProtoBuf.ProtoMember(33)]
    public float renderTextureCount { get; set; }
    [ProtoBuf.ProtoMember(34)]
    public float renderTextureMemory { get; set; }
    [ProtoBuf.ProtoMember(35)]
    public float usedBufferCount { get; set; }
    [ProtoBuf.ProtoMember(36)]
    public float usedBufferMemory { get; set; }
    [ProtoBuf.ProtoMember(37)]
    public float vBufferCount { get; set; }
    [ProtoBuf.ProtoMember(38)]
    public float vBufferMemory { get; set; }
    [ProtoBuf.ProtoMember(39)]
    public float inBufferCount { get; set; }
    [ProtoBuf.ProtoMember(40)]
    public float inBufferMemory { get; set; }
    [ProtoBuf.ProtoMember(41)]
    public float shadowCasterCount { get; set; }
    [ProtoBuf.ProtoMember(42)]
    public float systemusedMemory { get; set; }
    [ProtoBuf.ProtoMember(43)]
    public float totalusedMemory { get; set; } //totalusedMemory
    [ProtoBuf.ProtoMember(44)]
    public float totalreservedMemory { get; set; }
    [ProtoBuf.ProtoMember(45)]
    public float untrackedMemory { get; set; }
    [ProtoBuf.ProtoMember(46)]
    public float ManagedUsedHeap { get; set; }
    [ProtoBuf.ProtoMember(47)]
    public float ManagedReservedHeap { get; set; }
    [ProtoBuf.ProtoMember(48)]
    public float GraphicsUsedGD { get; set; }
    [ProtoBuf.ProtoMember(49)]
    public float audioUsed { get; set; }
    [ProtoBuf.ProtoMember(50)]
    public float videoUsed { get; set; }
    [ProtoBuf.ProtoMember(51)]
    public float profilerUsed { get; set; }
    [ProtoBuf.ProtoMember(52)]
    public float profilerReserved { get; set; }
    [ProtoBuf.ProtoMember(53)]
    public float otherReserved { get; set; }
    [ProtoBuf.ProtoMember(54)]
    public float otherUsed { get; set; }
    [ProtoBuf.ProtoMember(55)]
    public float textureCount { get; set; }
    [ProtoBuf.ProtoMember(56)]
    public float textureSize { get; set; }
    [ProtoBuf.ProtoMember(57)]
    public float meshCount { get; set; }
    [ProtoBuf.ProtoMember(58)]
    public float meshSize { get; set; }
    [ProtoBuf.ProtoMember(59)]
    public float matCount { get; set; }
    [ProtoBuf.ProtoMember(60)]
    public float matSize { get; set; }
    [ProtoBuf.ProtoMember(61)]
    public float animationCount { get; set; }
    [ProtoBuf.ProtoMember(62)]
    public float animaionSize { get; set; }
    [ProtoBuf.ProtoMember(63)]
    public float assetesCount { get; set; }
    [ProtoBuf.ProtoMember(64)]
    public float gameobjCount { get; set; }
    [ProtoBuf.ProtoMember(65)]
    public float sceneobjCount { get; set; }
    [ProtoBuf.ProtoMember(66)]
    public float objCount { get; set; }
    [ProtoBuf.ProtoMember(67)]
    public float gcframeCount { get; set; }
    [ProtoBuf.ProtoMember(68)]
    public float gcframeSize { get; set; }
    [ProtoBuf.ProtoMember(69)]
    public float physicsUsedMemory { get; set; }
    [ProtoBuf.ProtoMember(70)]
    public float dynamicBodies { get; set; }
    [ProtoBuf.ProtoMember(71)]
    public float articulationBodies { get; set; }
    [ProtoBuf.ProtoMember(72)]
    public float activeDynamicBodies { get; set; }
    [ProtoBuf.ProtoMember(73)]
    public float activeKinematicBodies { get; set; }
    [ProtoBuf.ProtoMember(74)]
    public float activeConstraints { get; set; }
    [ProtoBuf.ProtoMember(75)]
    public float staticColliders { get; set; }
    [ProtoBuf.ProtoMember(76)]
    public float collidersSynced { get; set; }
    [ProtoBuf.ProtoMember(77)]
    public float rigidbodiesSynced { get; set; }
    [ProtoBuf.ProtoMember(78)]
    public float physicsQueries { get; set; }
}
