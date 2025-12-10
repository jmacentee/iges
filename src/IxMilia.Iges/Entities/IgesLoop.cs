using System.Collections.Generic;
using System;

namespace IxMilia.Iges.Entities
{
    public class IgesLoop : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Loop;

        public List<IgesEntity>? Curves { get; set; }
        public List<int> EdgePointers { get; set; } = new List<int>();  // Store actual edge pointers from file
        public bool IsOuter { get; set; }

        private IgesReaderBinder? _binder;

        public IgesLoop() {}

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            int index = 0;
            _binder = binder;
            
            // IGES Loop (type 510) standard format according to IGES spec:
            // 1. Type of curve (usually 1 for edge)
            // 2. Curve entity pointer
            // 3. Orientation (1 or -1)
            // Repeat for each edge in the loop
            // Last parameter: IsOuter flag (1 = outer, 0 = inner)
            
            Curves = new List<IgesEntity>();
            EdgePointers.Clear();
            
            // Read edge pointers from the parameters
            // Standard format: [curve_type, curve_ptr, orientation, ...]
            // We need to skip type indicators and read actual pointers
            while (index < parameters.Count)
            {
                int value = Integer(parameters, index++);
                
                // Check if this looks like an orientation flag (last parameter, value 0 or 1)
                if (index >= parameters.Count - 1 && (value == 0 || value == 1))
                {
                    IsOuter = (value == 1);
                    break;
                }
                
                // If we have at least 2 more parameters, this might be (type, pointer, orientation)
                if (index + 1 < parameters.Count)
                {
                    int nextVal = Integer(parameters, index);
                    // If next value looks like a pointer (> 10) and current value is small (type indicator)
                    if (nextVal > 10 && value <= 3)
                    {
                        // This is likely (curve_type, pointer, orientation)
                        int curvePointer = nextVal;
                        int orientation = (index + 1 < parameters.Count) ? Integer(parameters, index + 1) : 1;
                        index += 2;
                        
                        EdgePointers.Add(curvePointer);
                        
                        // Try to bind immediately (may fail if entity not yet loaded)
                        binder.BindEntity(curvePointer, e => {
                            if (e != null && !Curves.Contains(e))
                                Curves.Add(e);
                        });
                    }
                }
            }
            
            return parameters.Count;
        }

        internal override void WriteParameters(List<object?> parameters, IgesWriterBinder binder)
        {
            if (Curves != null)
            {
                foreach (var curve in Curves)
                {
                    parameters.Add(1);  // Curve type
                    parameters.Add(binder.GetEntityId(curve));
                    parameters.Add(1);  // Orientation
                }
            }
            parameters.Add(IsOuter ? 1 : 0);
        }
    }
}