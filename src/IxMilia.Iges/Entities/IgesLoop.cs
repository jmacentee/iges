using System.Collections.Generic;
using System;

namespace IxMilia.Iges.Entities
{
    public class IgesLoop : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Loop;

        public List<IgesEntity>? Curves { get; set; }
        public List<int> CurvePointers { get; set; } = new List<int>();  // Store pointers for later resolution
        public bool IsOuter { get; set; }

        public IgesLoop() {}

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            int index = 0;
            
            // IGES Loop type 510 format
            // Parameters: edge_count, (edge_ptr, orientation, ...)*
            // Note: Plasticity exports have a non-standard format, so we'll just try to read what's there
            
            int edgeCount = Integer(parameters, index++);
            
            Curves = new List<IgesEntity>();
            CurvePointers.Clear();
            
            // Read edge/curve pointers with orientation flags
            for (int i = 0; i < edgeCount && index < parameters.Count; i++)
            {
                int curvePointer = Integer(parameters, index++);
                int orientation = (index < parameters.Count) ? Integer(parameters, index++) : 0;
                
                CurvePointers.Add(curvePointer);
                
                // Deferred binding
                binder.BindEntity(curvePointer, e => {
                    if (e != null && !Curves.Contains(e))
                        Curves.Add(e);
                });
            }
            
            // IsOuter is typically the last parameter
            IsOuter = false;
            if (index < parameters.Count)
            {
                IsOuter = Integer(parameters, index++) == 1;
            }
            
            return parameters.Count;
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
            parameters.Add(IsOuter ? 1 : 0);
        }
    }
}