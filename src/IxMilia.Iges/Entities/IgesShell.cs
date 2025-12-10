using System;
using System.Collections.Generic;

namespace IxMilia.Iges.Entities
{
    public class IgesShell : IgesEntity
    {
        public override IgesEntityType EntityType => IgesEntityType.Shell;

        // Store raw pointers for post-loading resolution
        private List<int>? _facePointers;
        private List<int>? _faceOrientations;

        // Post-binding: populated faces
        public List<IgesFace>? Faces { get; set; }

        public IgesShell() { }

        internal override int ReadParameters(List<string> parameters, IgesReaderBinder binder)
        {
            int index = 0;
            int faceCount = Integer(parameters, index++);
            
            _facePointers = new List<int>();
            _faceOrientations = new List<int>();

            for (int i = 0; i < faceCount; i++)
            {
                int facePointer = Integer(parameters, index++);
                int orientation = Integer(parameters, index++);
                
                _facePointers.Add(facePointer);
                _faceOrientations.Add(orientation);
            }
            
            return index;
        }

        internal override void WriteParameters(List<object?> parameters, IgesWriterBinder binder)
        {
            parameters.Add(Faces?.Count ?? 0);
            if (Faces != null)
            {
                foreach (var face in Faces)
                {
                    parameters.Add(binder.GetEntityId(face));
                    parameters.Add(1);  // orientation flag
                }
            }
        }

        /// <summary>
        /// Resolves face pointers using the provided entity line number map.
        /// IGES entity pointers in Shell parameters point to the SECOND line (even-numbered line) of directory entries.
        /// We need to subtract 2 to get the first line (odd-numbered line) which is where the entity starts.
        /// </summary>
        public void ResolveFacePointers(Dictionary<int, IgesEntity> entityByLineNumber)
        {
            Faces = new List<IgesFace>();

            if (_facePointers == null || _facePointers.Count == 0)
                return;

            foreach (int facePointer in _facePointers)
            {
                // IGES Shell pointers point to the second line (even) of directory entries
                // Subtract 2 to get the first line (odd) where the entity actually starts
                int directoryLineNumber = facePointer - 2;
                
                if (entityByLineNumber.TryGetValue(directoryLineNumber, out var entity) && entity is IgesFace face)
                {
                    Faces.Add(face);
                }
            }
        }
    }
}