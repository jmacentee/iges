using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    public class IgesFace : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Face;

        // Added for LaserConvert compatibility
        public IgesEntity? Surface { get; set; }
        public List<IgesLoop>? Loops { get; set; }
        
        // Store the surface pointer and loop pointers for later binding
        private int _surfacePointer;
        private List<int> _loopPointers = new List<int>();
        private IgesReaderBinder? _binder;

        public IgesFace() {}

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            int index = 0;
            // Surface pointer
            _surfacePointer = Integer(parameters, index++);
            _binder = binder;
            
            // Loop count
            int loopCount = Integer(parameters, index++);
            Loops = new List<IgesLoop>();
            _loopPointers.Clear();
            
            // If loopCount > 0, the next loopCount parameters are loop pointers
            for (int i = 0; i < loopCount; i++)
            {
                int loopPointer = Integer(parameters, index++);
                _loopPointers.Add(loopPointer);
            }
            
            return index;
        }

        internal override void OnAfterRead(IgesDirectoryData directoryData)
        {
            // Now bind the surface pointer (after all entities have been registered)
            if (_surfacePointer > 0 && _binder != null)
            {
                _binder.BindEntity(_surfacePointer, e => Surface = e);
            }
            
            // Bind loop pointers if we have any
            foreach (var loopPointer in _loopPointers)
            {
                if (loopPointer > 0 && _binder != null)
                {
                    _binder.BindEntity(loopPointer, e => {
                        if (e is IgesLoop loop)
                            Loops.Add(loop);
                    });
                }
            }
            
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