using System;
using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    public class IgesFace : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Face;

        // Added for LaserConvert compatibility
        public IgesEntity? Surface { get; set; }
        public List<IgesLoop>? Loops { get; set; }
        public List<int> EdgePointers { get; set; } = new List<int>();
        public List<IgesEntity> Edges { get; set; } = new List<IgesEntity>();  // Resolved edges
        
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
            
            // Next parameter: loop count (standard IGES)
            int loopCount = Integer(parameters, index++);
            Loops = new List<IgesLoop>();
            _loopPointers.Clear();
            EdgePointers.Clear();
            
            // Skip any flag/padding
            if (index < parameters.Count && Integer(parameters, index) == 0)
            {
                index++;
            }
            
            // Extract edge pointers from the remaining parameters
            // Plasticity pattern: edge_ptr, orientation, 0, 0, 0, edge_ptr, orientation, 0, 0, 0, ...
            while (index + 4 < parameters.Count)
            {
                int p1 = Integer(parameters, index);
                int p2 = Integer(parameters, index + 1);
                int p3 = Integer(parameters, index + 2);
                int p4 = Integer(parameters, index + 3);
                int p5 = Integer(parameters, index + 4);
                
                // Look for pattern: X, any, 0, 0, 0 where X might be edge pointer
                if (p1 > 10 && p3 == 0 && p4 == 0 && p5 == 0)
                {
                    EdgePointers.Add(p1);
                    index += 5;
                }
                else
                {
                    index++;
                }
            }
            
            // Store loop pointers for standard IGES binding
            for (int i = 0; i < loopCount && index < parameters.Count; i++)
            {
                int loopPointer = Integer(parameters, index++);
                _loopPointers.Add(loopPointer);
            }
            
            return parameters.Count;
        }

        internal override void OnAfterRead(IgesDirectoryData directoryData)
        {
            // Bind the surface pointer with offset matching
            if (_surfacePointer > 0 && _binder != null)
            {
                _binder.BindEntity(_surfacePointer, e => {
                    if (e is IgesPlaneSurface || e is IgesPlane)
                        Surface = e;
                });
                
                if (Surface == null)
                {
                    for (int offset = 1; offset <= 50; offset++)
                    {
                        int adjustedPtr = _surfacePointer - offset;
                        if (adjustedPtr > 0)
                        {
                            _binder.BindEntity(adjustedPtr, e => {
                                if (Surface == null && (e is IgesPlaneSurface || e is IgesPlane))
                                    Surface = e;
                            });
                        }
                        if (Surface != null) break;
                    }
                }
            }
            
            // Bind edge pointers with intelligent offset matching
            foreach (var edgePtr in EdgePointers)
            {
                if (edgePtr > 0 && _binder != null)
                {
                    bool found = false;
                    
                    // First try: direct pointer
                    _binder.BindEntity(edgePtr, e => {
                        if (!found && (e is IgesLine || e is IgesCircularArc || e is IgesRationalBSplineCurve || e is IgesCompositeCurve))
                        {
                            if (!Edges.Contains(e))
                            {
                                Edges.Add(e);
                                found = true;
                            }
                        }
                    });
                    
                    // If not found, try systematic offsets
                    if (!found)
                    {
                        for (int offset = 1; offset <= 100; offset++)
                        {
                            int adjustedPtr = edgePtr - offset;
                            if (adjustedPtr > 0)
                            {
                                _binder.BindEntity(adjustedPtr, e => {
                                    if (!found && (e is IgesLine || e is IgesCircularArc || e is IgesRationalBSplineCurve || e is IgesCompositeCurve))
                                    {
                                        if (!Edges.Contains(e))
                                        {
                                            Edges.Add(e);
                                            found = true;
                                        }
                                    }
                                });
                            }
                            
                            if (found) break;
                        }
                    }
                }
            }
            
            // Bind loop pointers (standard IGES)
            foreach (var loopPointer in _loopPointers)
            {
                if (loopPointer > 0 && _binder != null)
                {
                    _binder.BindEntity(loopPointer, e => {
                        if (e is IgesLoop loop && !Loops.Contains(loop))
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