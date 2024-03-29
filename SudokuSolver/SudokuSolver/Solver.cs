﻿using System.Drawing;
using System.Text;

namespace SudokuSolver
{
    /// <summary>The sudoku solver class.</summary>
    public class Solver
    {
        public const int BoardSize = 15;
        public const int InnerAreaRowLength = 5;
        public const int InnerAreaColLength = 3;

        private static readonly IEnumerable<int> AllPossibleValues = Enumerable.Range(1, BoardSize);

        /// <summary>Finds all solutions to the given sudoku, and returns a string with all sudoku elements, in order.</summary>
        /// <param name="sudoku">The sudoku to solve.</param>
        /// <returns>The list of possible solutions.</returns>
        public static List<string> ComputeSolutions(int[][] sudoku)
        {
            var solutions = new List<string>();
            AssignValueToNode(sudoku, 0, 0, new Dictionary<Point, int>(), solutions);

            return solutions;
        }

        /// <summary>Takes in a solution string, which consists in sudoku elements in order, separated by a space.</summary>
        /// <param name="solution">The solution string.</param>
        /// <returns>The sudoku elements in a 9x9 grid.</returns>
        public static string FormatSolution(string solution)
            => solution.Batch(2 * BoardSize).Aggregate(new StringBuilder(), (acc, item) => acc.AppendLine(string.Join("", item))).ToString();

        private static void AssignValueToNode(int[][] initialSudoku, int row, int col, Dictionary<Point, int> optionsSoFar, List<string> solutions)
        {
            var shouldGoToNextRow = col == BoardSize;
            if (shouldGoToNextRow)
            {
                col = 0;
                ++row;
            }

            var reachedEndOfBoard = row == BoardSize;
            if (reachedEndOfBoard) // base condition for end of sudoku board; this should be reached only for valid solutions
            {
                solutions.Add(SolutionToString(optionsSoFar));
                return;
            }

            var possibleValuesForNode = ComputePossibleValuesForNode(initialSudoku, row, col, optionsSoFar);
            if (!possibleValuesForNode.Any()) // base condition for invalid solution
            {
                return;
            }

            foreach (var possibleValue in possibleValuesForNode)
            {
                var newOptionsSoFar = new Dictionary<Point, int>(optionsSoFar);
                newOptionsSoFar.Add(new Point(row, col), possibleValue);

                AssignValueToNode(initialSudoku, row, col + 1, newOptionsSoFar, solutions);
            }
        }

        private static IEnumerable<int> ComputePossibleValuesForNode(int[][] initialSudoku, int row, int col, Dictionary<Point, int> optionsSoFar)
        {
            if (initialSudoku[row][col] != 0)
            {
                return new List<int> { initialSudoku[row][col] };
            }

            var currentValuesRelevantForNode = GetCurrentSudokuValuesRelevantForCoordinates(initialSudoku, row, col, optionsSoFar);
            var possibleValuesForNode = AllPossibleValues.Except(currentValuesRelevantForNode);

            return possibleValuesForNode;
        }

        private static IEnumerable<int> GetCurrentSudokuValuesRelevantForCoordinates(int[][] initialSudoku, int row, int col, Dictionary<Point, int> optionsSoFar)
        {
            var initialValues = GetInitialValues(initialSudoku, row, col);
            var relevantValuesFromOptions = GetRelevantValuesFromOptions(optionsSoFar, row, col);
            var existingValues = initialValues.Concat(relevantValuesFromOptions).Distinct();

            return existingValues;
        }

        private static IEnumerable<int> GetRelevantValuesFromOptions(Dictionary<Point, int> optionsSoFar, int row, int col)
        {
            var rowValues = optionsSoFar.Where(option => option.Key.X == row).Select(option => option.Value);
            var colValues = optionsSoFar.Where(option => option.Key.Y == col).Select(option => option.Value);
            var squareCoordinates = GetSquareCoordinates(row, col);
            var squareValues = optionsSoFar.Where(option => squareCoordinates.Any(squareCoord => squareCoord.Row == option.Key.X && squareCoord.Col == option.Key.Y)).Select(option => option.Value);
            var relevantValuesForOptions = rowValues.Concat(colValues).Concat(squareValues).Distinct();

            return relevantValuesForOptions;
        }

        private static IEnumerable<int> GetInitialValues(int[][] initialSudoku, int row, int col)
        {
            var initialRowValues = GetValuesFromRow(initialSudoku, row);
            var initialColValues = GetValuesFromColumn(initialSudoku, col);
            var initialSquareValues = GetValuesFromSquare(initialSudoku, row, col);
            var initialValues = initialRowValues.Concat(initialColValues).Concat(initialSquareValues).Distinct();

            return initialValues;
        }

        private static IEnumerable<int> GetValuesFromRow(int[][] sudokuBoard, int rowIndex)
            => Enumerable.Range(0, BoardSize).Select(columnIndex => sudokuBoard[rowIndex][columnIndex]);

        private static IEnumerable<int> GetValuesFromColumn(int[][] sudokuBoard, int columnIndex)
            => Enumerable.Range(0, BoardSize).Select(rowIndex => sudokuBoard[rowIndex][columnIndex]);

        private static IEnumerable<int> GetValuesFromSquare(int[][] sudokuBoard, int rowIndex, int columnIndex)
            => GetSquareCoordinates(rowIndex, columnIndex).Select(pair => sudokuBoard[pair.Row][pair.Col]);

        private static IEnumerable<(int Row, int Col)> GetSquareCoordinates(int rowIndex, int columnIndex)
        {
            var originX = rowIndex / InnerAreaColLength * InnerAreaColLength;
            var originY = columnIndex / InnerAreaRowLength * InnerAreaRowLength;

            var rowMax = originX + InnerAreaColLength;
            var colMax = originY + InnerAreaRowLength;

            var result = new List<(int, int)>();
            for (int i = originX; i < rowMax; i++)
            {
                for (int j = originY; j < colMax; j++)
                {
                    result.Add((i, j));
                }
            }

            return result;
        }

        private static string SolutionToString(Dictionary<Point, int> options)
            => string.Join(' ', options.Values);
    }
}
