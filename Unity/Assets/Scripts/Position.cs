using System;

public struct Position
{
	public int row;
	public int column;

	public Position(int row, int column) {
		this.row = row;
		this.column = column;
	}

	public override string ToString() {
		return string.Format ("Position[row = {0}, column = {1}]", row, column);
	}

	public Position translate(int offsetRow, int offsetColumn) {
		return new Position {
			row = row + offsetRow,
			column = column + offsetColumn
		};
	}
}

