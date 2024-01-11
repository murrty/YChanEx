namespace SoftCircuits.HtmlMonkey;
internal sealed class TagStack : System.Collections.Generic.Stack<string> {
    /*
    private string[] _array;
    private int _size;
    private int _version;

    private const int DefaultCapacity = 4;

    public TagStack() {
        _array = [];
    }

    public TagStack(int capacity) {
        Throw.IfNegative(capacity);
        _array = new string[capacity];
    }

    public TagStack(IEnumerable<string> collection) {
        Throw.IfNull(collection);
        if (collection is ICollection<string> ic) {
            int count = ic.Count;
            if (count != 0) {
                string[] arr = new string[count];
                ic.CopyTo(arr, 0);
                _size = count;
            }
        }
        else {
            using var en = collection.GetEnumerator();
            if (en.MoveNext()) {
                _array = new string[DefaultCapacity];
                _array[0] = en.Current;
                _size++;
                while (en.MoveNext()) {
                    if (_size == _array.Length) {
                        int newSize = _size << 1;
                        Throw.IfGreaterThan((uint)newSize, (uint)int.MaxValue);
                        //if ((uint)newSize > Array.MaxLength) {
                        //    newSize = Array.MaxLength <= _size ? _size + 1 : Array.MaxLength;
                        //}
                        Array.Resize(ref _array, newSize);
                    }

                    _array[_size++] = en.Current;
                }

            }
            else {
                _size = 0;
                _array = [];
            }
        }
    }
    */
}
