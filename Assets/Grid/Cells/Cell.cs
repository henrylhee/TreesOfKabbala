using System.Collections;
using System.Collections.Generic;

public class Cell
{
    public CellTypes type { get; private set; }

    private int size;
    private int meshDetail;

    public Cell(int size, int meshDetail, CellTypes type)
    {      
        this.size = size;
        this.meshDetail = meshDetail;
        this.type = type;
    }


}
