using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    public class IgesLoop : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Loop;

        // Added for LaserConvert compatibility
        public List<IgesEntity>? Curves { get; set; }
        public bool IsOuter { get; set; }
        
        // Store curve pointers for later binding
        private List<int> _curvePointers = new List<int>();
        private IgesReaderBinder? _binder;

        public IgesLoop() {}

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            int index = 0;
            
            int firstParam = Integer(parameters, index++);
            
            Curves = new List<IgesEntity>();
            _curvePointers.Clear();
            _binder = binder;
            
            // Parse pairs of (pointer, orientation)
            while (index < parameters.Count)
            {
                int pointer = Integer(parameters, index++);
                
                if (index == parameters.Count)
                {
                    IsOuter = (pointer == 1);
                    break;
                }
                
                if (pointer > 0)
                {
                    int orientation = Integer(parameters, index++);
                    _curvePointers.Add(pointer);
                }
                else
                {
                    int _ = Integer(parameters, index++);
                }
            }
            
            return parameters.Count;
        }

        internal override void OnAfterRead(IgesDirectoryData directoryData)
        {
            // Bind all curve/edge pointers
            foreach (var curvePointer in _curvePointers)
            {
                if (curvePointer > 0 && _binder != null)
                {
                    _binder.BindEntity(curvePointer, e => {
                        if (e != null)
                        {
                            Curves.Add(e);
                        }
                    });
                }
            }
            
            base.OnAfterRead(directoryData);
        }

        internal override void WriteParameters(List<object?> parameters, IgesWriterBinder binder)
        {
            parameters.Add(Curves?.Count ?? 0);
            if (Curves != null)
            {
                foreach (var curve in Curves)
                {
                    parameters.Add(binder.GetEntityId(curve));
                }
            }
            // Write IsOuter flag (0 = inner, 1 = outer)
            parameters.Add(IsOuter ? 1 : 0);
        }
    }
}