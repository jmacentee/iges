using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    public class IgesFace : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Face;

        // Added for LaserConvert compatibility
        public IgesEntity? Surface { get; set; }
        public List<IgesLoop>? Loops { get; set; }
        
        // Store the surface pointer for later binding
        private int _surfacePointer;
        private IgesReaderBinder? _binder;

        public IgesFace() {}

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            int index = 0;
            // Surface pointer - just read it, don't bind yet
            _surfacePointer = Integer(parameters, index++);
            _binder = binder;
            
            System.Console.WriteLine($"[IGESFACE] ReadParameters: SurfacePointer={_surfacePointer}");
            
            // Loop count
            int loopCount = Integer(parameters, index++);
            Loops = new List<IgesLoop>();
            for (int i = 0; i < loopCount; i++)
            {
                int loopPointer = Integer(parameters, index++);
                binder.BindEntity(loopPointer, e => {
                    if (e is IgesLoop loop)
                        Loops.Add(loop);
                });
            }
            return index;
        }

        internal override void OnAfterRead(IgesDirectoryData directoryData)
        {
            System.Console.WriteLine($"[IGESFACE] OnAfterRead: DirectorySeqNum={directoryData.SequenceNumber}, SurfacePointer={_surfacePointer}, Surface before binding={Surface?.GetType().Name ?? "null"}");
            
            // Now bind the surface pointer (after all entities have been registered)
            if (_surfacePointer > 0 && _binder != null)
            {
                _binder.BindEntity(_surfacePointer, e => {
                    System.Console.WriteLine($"[IGESFACE] OnAfterRead bind callback: Entity type={e?.GetType().Name ?? "null"} for pointer {_surfacePointer}");
                    Surface = e;
                });
            }
            System.Console.WriteLine($"[IGESFACE] OnAfterRead: Surface after binding={Surface?.GetType().Name ?? "null"}");
            
            base.OnAfterRead(directoryData);
        }

        internal override void WriteParameters(List<object?> parameters, IgesWriterBinder binder)
        {
            // Write Surface pointer
            parameters.Add(binder.GetEntityId(Surface));
            // Write Loop count
            parameters.Add(Loops?.Count ?? 0);
            // Write Loop pointers
            if (Loops != null)
            {
                foreach (var loop in Loops)
                {
                    parameters.Add(binder.GetEntityId(loop));
                }
            }
        }
    }
}