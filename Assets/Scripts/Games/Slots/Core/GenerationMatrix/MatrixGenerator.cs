using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Casino
{
    public class SymbolMatrix
    {
        public string[][] symbolTags;
    }

    public enum RewardType
    {
        LineDefault,
        LineBig,
        Scatter,
    }

    public class SpinRewardStep
    {
        public RewardType rewardType;
        public SlotLine lineConfig;
        public float reward;
    }

    public class FinalSpinReward
    {
        public SymbolMatrix matrix;
        public List<SpinRewardStep> rewardSteps;
        public float allReward;
    }

    public class SymbolInWork
    {
        public uint symbolMask = ~(uint)0;
        public bool fixedSymbol = false;

        public void Set(SymbolInWork src)
        {
            symbolMask = src.symbolMask;
            fixedSymbol = src.fixedSymbol;
        }

        public SymbolInWork Copy()
        {
            SymbolInWork newSymbol = new SymbolInWork();
            newSymbol.symbolMask = symbolMask;
            newSymbol.fixedSymbol = fixedSymbol;
            return newSymbol;
        }
    }

    public class MatrixInWork
    {
        public SymbolInWork[][] symbols;

        public MatrixInWork(int drumCount, int maxDrumHeight, bool createSymbols = true)
        {
            symbols = new SymbolInWork[drumCount][];
            for (int i = 0; i < symbols.Length; i++)
            {
                symbols[i] = new SymbolInWork[maxDrumHeight];
                if (createSymbols)
                {
                    for (int j = 0; j < maxDrumHeight; j++)
                    {
                        symbols[i][j] = new SymbolInWork();
                    }
                }
            }
        }

        public void Set(MatrixInWork src)
        {
            if (src.symbols.Length != symbols.Length)
                return;

            for (int i = 0; i < src.symbols.Length; i++)
            {
                if (src.symbols[i].Length != symbols[i].Length)
                    return;
            }

            for (int i = 0; i < src.symbols.Length; i++)
            {
                for (int j = 0; j < src.symbols[i].Length; j++)
                {
                    symbols[i][j].Set(src.symbols[i][j]);
                }
            }
        }

        public MatrixInWork Copy()
        {
            MatrixInWork newMatrix = new MatrixInWork(symbols.Length, symbols[0].Length, false);

            for (int i = 0; i < symbols.Length; i++)
            {
                for (int j = 0; j < symbols[i].Length; j++)
                {
                    newMatrix.symbols[i][j] = symbols[i][j].Copy();
                }
            }

            return newMatrix;
        }
    }

    public enum SymbolStepResultType
    {
        None,
        SomethingWrong,
        Success,
        NoAvaliableLines,
        LinesNotFound,
        EstimateRewardIsSmall
    }

    public enum FinalStepResultType
    {
        None,
        SomethingWrong,
        Success,
        LineCombinationsFailed,
        AllLinesFailed,
        IncorrectReward,
        FinalRewardIsBig,
        FinalRewardIsSmall,
        IncreaseSymbolReward
    }

    public class GenLineId
    {
        public uint lineId;
        public SlotLine lineConfig;
    }

    public class GenSymbolId
    {
        public uint symbolId;
        public SlotSymbol symbolConfig;
    }

    public class MatrixStepBySymbol
    {
        public GenSymbolId symbolId;
        public MatrixInWork matrix;
        public SlotLine lineCombination;
        public SymbolCombination combination;
        public float currentReward;
    }

    public class MatrixGenerator : MonoBehaviour
    {
        private Dictionary<uint, GenLineId> lineIdToGenLine = new Dictionary<uint, GenLineId>();
        private Dictionary<uint, GenSymbolId> symbolIdToGenSymbol = new Dictionary<uint, GenSymbolId>();

        private SpinInfo currentSpinInfo;

        public FinalSpinReward GenerateMatrix(SpinInfo spinInfo)
        {
            int i = 0;
            symbolIdToGenSymbol.Clear();
            foreach (var curSymbol in SlotsGame.Instance.SlotMachineConfig.symbols)
            {
                uint symbolOffset = (uint)1 << i++;
                symbolIdToGenSymbol.Add(symbolOffset, new GenSymbolId() { symbolConfig = curSymbol, symbolId = symbolOffset });
            }

            i = 0;
            lineIdToGenLine.Clear();
            foreach (var curLine in SlotsGame.Instance.SlotMachineConfig.lines)
            {
                uint lineOffset = (uint)1 << i++;
                lineIdToGenLine.Add(lineOffset, new GenLineId() {lineConfig = curLine, lineId = lineOffset});
            }

            currentSpinInfo = spinInfo;

            MatrixInWork newMatrix = new MatrixInWork(SlotsGame.Instance.SlotMachineConfig.drums.Length, SlotsGame.Instance.SlotMachineConfig.MaxDrumHeight());

            //newMatrix.symbols[1][1].fixedSymbol = true;

            //FinalSpinReward finalSpinReward = new FinalSpinReward();

            int[] firstIndicies = new int[newMatrix.symbols[0].Length];
            for (int j = 0; j < firstIndicies.Length; j++)
                firstIndicies[j] = j;

            Shuffle(firstIndicies);

            ProcessSymbols(firstIndicies, newMatrix);


            return new FinalSpinReward();
        }

        private Dictionary<int, float> randomsForMultiSymbols = new Dictionary<int, float>()
        {
            {2, 8.0f},
            {3, 4.0f},
            {4, 2.0f},
            {5, 1.0f}
        };

        class MultiplyFactorChance
        {
            public float originalChance;
            public float chanceOffset;
            public SymbolCombination combination;
        }

        List<MultiplyFactorChance> CalculateFactorChances(SymbolCombination[] combinations, float chanceModificator)
        {
            List<MultiplyFactorChance> factorChances = new List<MultiplyFactorChance>();

            float minCount = combinations[0].count;
            float maxCount = combinations.Last().count;
            float middleCount = (minCount + maxCount) / 2.0f - 0.00001f;
            if (chanceModificator < -15.0f)
            {
                factorChances.Add(new MultiplyFactorChance() { originalChance = 100.0f, combination = combinations[0] });
            }
            else if (chanceModificator > 15.0f)
            {
                factorChances.Add(new MultiplyFactorChance() { originalChance = 100.0f, combination = combinations.Last() });
            }
            else
            {
                foreach (var curCombination in combinations)
                {
                    float foundChance = 0.0f;
                    if (randomsForMultiSymbols.TryGetValue(curCombination.count, out foundChance))
                    {
                        float curChance = foundChance;
                        if (chanceModificator < -0.0001f && curCombination.count < middleCount)
                            curChance *= chanceModificator;
                        else if (chanceModificator > 0.0001f && curCombination.count > middleCount)
                            curChance *= chanceModificator;

                        factorChances.Add(new MultiplyFactorChance() { originalChance = curChance, combination = curCombination });
                    }
                }
            }

            float allChances = 0.0f;
            foreach (var curChance in factorChances)
            {
                allChances += curChance.originalChance;
            }

            allChances = 100.0f / allChances;

            float lastOffset = 0.0f;
            for (int i = 0; i < factorChances.Count; i++)
            {
                factorChances[i].originalChance *= allChances; //normalize chances
                factorChances[i].chanceOffset = factorChances[i].originalChance + lastOffset;

                lastOffset = factorChances[i].chanceOffset;
            }

            return factorChances;
        }

        SymbolCombination GetRandomCombination(List<MultiplyFactorChance> chances)
        {
            float randomValue = UnityEngine.Random.value * 100.0f;
            for (int i = 0; i < chances.Count; i++)
            {
                if (randomValue <= (chances[i].chanceOffset))
                    return chances[i].combination;
            }

            return chances.Last().combination;
        }

        void ProcessSymbols(int[] firstIndicies, MatrixInWork startMatrix)
        {
            List <MatrixStepBySymbol> steps = new List<MatrixStepBySymbol>();

            int currentIndex = 0;
            float symbolRewardFactor = 1.0f;
            float curReward = 0.0f;

            float rewardChanceModifier = 1.0f;
            float maxRewardChanceValue = 16.0f;

            List<string> excludedLines = new List<string>();
            int countClearStart = 0;
            int countIterations = 0;

            List<GenSymbolId> symbolsToGenerate = symbolIdToGenSymbol.Values.ToList();


            while (true)
            {
                countIterations++;

                if (currentIndex >= firstIndicies.Length)
                    currentIndex = 0;

                GenSymbolId currentSymbol = symbolsToGenerate[Random.Range(0, symbolsToGenerate.Count)];
                MatrixInWork curMatrix;
                if (steps.Count == 0)
                {
                    curMatrix = startMatrix;
                    countClearStart++;
                }
                else
                {
                    curMatrix = steps.Last().matrix;
                }

                float needAllReward = (currentSpinInfo.finalRewardPercent - curReward);
                MatrixStepBySymbol newStep;
                SymbolStepResultType result = FindMatrixForSymbol(firstIndicies[currentIndex], currentSymbol, curMatrix, needAllReward, rewardChanceModifier, excludedLines, out newStep);

                //если успешно добавили новую линию
                if (result == SymbolStepResultType.Success)
                {
                    float newFinalReward = curReward + newStep.currentReward;
                    
                    //если достигли требуемой награды, прекращаем алгоритм
                    if (Mathf.Abs(newFinalReward - currentSpinInfo.finalRewardPercent) < currentSymbol.symbolConfig.GetMinimumReward() * 0.5f)
                    {
                        Debug.LogError("Reward reached!");
                        curReward += newStep.currentReward;
                        excludedLines.Add(newStep.lineCombination.tag);
                        steps.Add(newStep);
                        break;
                    }
                    else if (newFinalReward > (currentSpinInfo.finalRewardPercent +
                                               currentSymbol.symbolConfig.GetMinimumReward()))
                    {

                        //если не получилось за несколько раз, выдаем текущий результат (хоть он и не валидный)
                        if (rewardChanceModifier < (-maxRewardChanceValue + 0.001f))
                        {
                            Debug.LogError("Reward is very big! Finished");
                            curReward += newStep.currentReward;
                            excludedLines.Add(newStep.lineCombination.tag);
                            steps.Add(newStep);
                            break;
                        }
                        else
                        {
                            //уменьшаем вероятность больших наград и начинаем сначала
                            steps.Clear();
                            currentIndex = 0;
                            curReward = 0.0f;
                            excludedLines.Clear();
                            rewardChanceModifier = Mathf.Min(rewardChanceModifier, -1.0f);
                            rewardChanceModifier = rewardChanceModifier * 2.0f;
                        }

                        continue;
                    }

                    curReward += newStep.currentReward;
                    currentIndex++;
                    steps.Add(newStep);
                    excludedLines.Add(newStep.lineCombination.tag);
                }
                else if (result == SymbolStepResultType.EstimateRewardIsSmall)
                {
                    //минимальная комбинация слишком большая для награды, которую осталось набрать, значит все ок, прерываем алгоритм.
                    break;
                }
                else if (result == SymbolStepResultType.NoAvaliableLines || result == SymbolStepResultType.LinesNotFound)
                {
                    //если не получилось за несколько раз, выдаем текущий результат (хоть он и не валидный)
                    if (rewardChanceModifier > (maxRewardChanceValue - 0.0001f))
                    {
                        Debug.LogError("Reward not reached, lines incorrect..");
                        break;
                    }
                    else
                    {
                        //уменьшаем вероятность больших наград и начинаем сначала
                        steps.Clear();
                        currentIndex = 0;
                        curReward = 0.0f;
                        excludedLines.Clear();
                        rewardChanceModifier = Mathf.Max(rewardChanceModifier, 1.0f);
                        rewardChanceModifier = rewardChanceModifier * 2.0f;
                    }

                    continue;
                }
                else
                {
                    Debug.LogError("Something wrong! Unknown SymbolStepResultType");
                    steps.Clear();
                    excludedLines.Clear();
                    currentIndex = 0;
                    curReward = 0.0f;
                    rewardChanceModifier = 0.0f;
                }
            }

            Shuffle(symbolsToGenerate);

            //убираем линии которые еще могут сыграть, также генерим первые символы, если они еще не созданы, таким образом, что они не сыграют
            foreach (var curFirstindex in firstIndicies)
            {
                MatrixInWork lastMatrix = steps.Last().matrix;
                SymbolInWork matrixSymbol = lastMatrix.symbols[0][curFirstindex];
                if (symbolIdToGenSymbol.ContainsKey(matrixSymbol.symbolMask))
                {
                    GenSymbolId foundSymbol;
                    if (symbolIdToGenSymbol.TryGetValue(matrixSymbol.symbolMask, out foundSymbol))
                    {
                        List<SlotLine> noRemoveLines;
                        bool isRemoved = RemoveLinesForSymbol(curFirstindex, foundSymbol, lastMatrix, excludedLines, out noRemoveLines);
                        if (!isRemoved && noRemoveLines != null)
                        {
                            AddPlayingLinesToSteps(foundSymbol, lastMatrix, noRemoveLines, steps, excludedLines);
                        }
                    }
                }
                else
                {
                    GenSymbolId symbolFound = null;
                    for (int j = 0; j < symbolsToGenerate.Count; j++)
                    {
                        GenSymbolId curSymbol = symbolsToGenerate[j];
                        if ((curSymbol.symbolId & matrixSymbol.symbolMask) != 0 && !CheckSymbolByFirstUsed(firstIndicies, lastMatrix, curSymbol))
                        {
                            lastMatrix.symbols[0][curFirstindex].symbolMask = curSymbol.symbolId;
                            symbolFound = curSymbol;
                            break;
                        }
                    }

                    if (symbolFound == null)
                    {
                        symbolFound = symbolsToGenerate[0];
                    }

                    List<SlotLine> noRemoveLines;
                    bool isRemoved = RemoveLinesForSymbol(curFirstindex, symbolFound, lastMatrix, excludedLines, out noRemoveLines);
                    if (!isRemoved && noRemoveLines != null)
                    {
                        AddPlayingLinesToSteps(symbolFound, lastMatrix, noRemoveLines, steps, excludedLines);
                    }
                }
            }

            //генерим оставшиеся символы, которые точно не сыграют
            MatrixInWork matrix = steps.Last().matrix;
            for (int i = 0; i < matrix.symbols.Length; i++)
            {
                for (int j = 0; j < matrix.symbols[i].Length; j++)
                {
                    SymbolInWork symbol = matrix.symbols[i][j];
                    if (!symbol.fixedSymbol)
                    {
                        Shuffle(symbolsToGenerate);
                        foreach (var curSymbolId in symbolsToGenerate)
                        {
                            if ((symbol.symbolMask & curSymbolId.symbolId) != 0)
                            {
                                symbol.symbolMask = curSymbolId.symbolId;
                                symbol.fixedSymbol = true;
                            }
                        }
                    }
                }
            }

            Debug.Log("Finish generate! Need reward=" + currentSpinInfo.finalRewardPercent + "; real reward=" + curReward + "; steps=" + steps.Count + "; clearStart=" + countClearStart + "; allIterations=" + countIterations);

            int k1 = 0;
            foreach (var curStep in steps)
            {
                Debug.Log("Step"+(k1++) + ": Symbol=" + curStep.symbolId.symbolConfig.tag + "; Line=" + curStep.lineCombination.ToString() + "; reward=" + curStep.combination.reward);
            }

            matrix = steps.Last().matrix;
            for (int i = 0; i < matrix.symbols.Length; i++)
            {
                string curLine = i + ": ";
                for (int j = 0; j < matrix.symbols[i].Length; j++)
                {
                    string curSymbolStr = "[";
                    foreach (var s in symbolsToGenerate)
                    {
                        if ((matrix.symbols[i][j].symbolMask & s.symbolId) == 0)
                            curSymbolStr += "<_" + s.symbolConfig.tag + "_>";
                        else
                            curSymbolStr += "<" + s.symbolConfig.tag + ">";
                    }

                    curSymbolStr = curSymbolStr.Trim(new char[] {','}) + "]";
                    curLine += curSymbolStr + ",";
                }
                curLine = curLine.Trim(new char[] { ',' }) + "]";

                Debug.Log("Matrix line " + curLine);
            }
        }

        bool CheckSymbolByFirstUsed(int[] firstIndicies, MatrixInWork matrix, GenSymbolId symbolId)
        {
            for (int i = 0; i < firstIndicies.Length; i++)
            {
                if (matrix.symbols[0][firstIndicies[i]].symbolMask == symbolId.symbolId)
                    return true;
            }

            return false;
        }

        SymbolStepResultType FindMatrixForSymbol(int posOnFirstDrum, GenSymbolId symbolId, MatrixInWork startMatrix, float maxEstimateReward, float chanceModificator, List<string> usedLines, out MatrixStepBySymbol result)
        {
            result = new MatrixStepBySymbol();

            List<GenLineId> linesForIndex = new List<GenLineId>();
            foreach (var curLine in lineIdToGenLine.Values)
            {
                if (curLine.lineConfig.indexOnDrum[0] == posOnFirstDrum && !usedLines.Contains(curLine.lineConfig.tag) && CheckForAvaliableLine(symbolId, curLine, startMatrix))
                    linesForIndex.Add(curLine);
            }

            if (linesForIndex.Count == 0)
                return SymbolStepResultType.NoAvaliableLines;

            float maxThreshold = symbolId.symbolConfig.GetMinimumReward() / 2.0f;

            var chances = CalculateFactorChances(symbolId.symbolConfig.combinations, chanceModificator);

            SymbolCombination selectedCombination = null;

            if (symbolId.symbolConfig.combinations[0].reward > (maxEstimateReward + maxThreshold))
            {
                return SymbolStepResultType.EstimateRewardIsSmall;
            }
            else if (Mathf.Abs(symbolId.symbolConfig.combinations[0].reward - maxEstimateReward) < maxThreshold)
            {
                selectedCombination = symbolId.symbolConfig.combinations[0];
            }

            while (selectedCombination == null)
            {
                SymbolCombination randomCombination = GetRandomCombination(chances);
                if (randomCombination.reward > (maxEstimateReward + maxThreshold) && chances[0].combination.reward < (maxEstimateReward + maxThreshold))
                    continue;

                selectedCombination = randomCombination;
            }

            Shuffle(linesForIndex);

            SlotLine resultLine = null;
            MatrixInWork newMatrix = startMatrix.Copy();
            foreach (var curLine in linesForIndex)
            {
                if (CheckLineToPrint(curLine.lineConfig, symbolId, selectedCombination, newMatrix))
                {
                    PrintSymbolLineToMatrix(curLine.lineConfig, symbolId, selectedCombination, newMatrix);
                    resultLine = curLine.lineConfig;
                    break;
                }
            }

            if (resultLine == null)
                return SymbolStepResultType.LinesNotFound;

            result.symbolId = symbolId;
            result.matrix = newMatrix;
            result.combination = selectedCombination;
            result.lineCombination = resultLine;
            result.currentReward = selectedCombination.reward;

            return SymbolStepResultType.Success;
        }

        bool RemoveLinesForSymbol(int posOnFirstDrum, GenSymbolId symbolId, MatrixInWork startMatrix, List<string> usedLines, out List<SlotLine> linesNotRemoved)
        {
            List<GenLineId> linesForIndex = new List<GenLineId>();
            foreach (var curLine in lineIdToGenLine.Values)
            {
                if (curLine.lineConfig.indexOnDrum[0] == posOnFirstDrum && !usedLines.Contains(curLine.lineConfig.tag) && CheckForAvaliableLine(symbolId, curLine, startMatrix))
                    linesForIndex.Add(curLine);
            }

            if (linesForIndex.Count == 0)
            {
                linesNotRemoved = null;
                return true;
            }

            linesNotRemoved = new List<SlotLine>();
            bool successRemoved = true;
            foreach (var curLine in linesForIndex)
            {
                if (!RemoveLine(curLine.lineConfig, symbolId, startMatrix))
                {
                    linesNotRemoved.Add(curLine.lineConfig);
                    successRemoved = false;
                }
            }

            return successRemoved;
        }

        void AddPlayingLinesToSteps(GenSymbolId symbolId, MatrixInWork startMatrix, List<SlotLine> lines, List<MatrixStepBySymbol> linesToPlay, List<string> usedLines)
        {
            foreach (var curLine in lines)
            {
                foreach (var curCombination in symbolId.symbolConfig.combinations)
                {
                    if (CheckLineToPrint(curLine, symbolId, curCombination, startMatrix))
                    {
                        MatrixStepBySymbol newStep = new MatrixStepBySymbol();
                        newStep.symbolId = symbolId;
                        newStep.matrix = startMatrix;
                        newStep.combination = curCombination;
                        newStep.currentReward = curCombination.reward;
                        newStep.lineCombination = curLine;
                        linesToPlay.Add(newStep);
                        usedLines.Add(curLine.tag);
                    }
                }
            }
        }

        bool RemoveLine(SlotLine lineId, GenSymbolId symbolId, MatrixInWork matrix)
        {
            int maxMajorDrums = symbolId.symbolConfig.combinations[0].count;
            for (int i = 1; i < maxMajorDrums; i++)
            {
                SymbolInWork workSymbol = matrix.symbols[i][lineId.indexOnDrum[i]];
                uint symbolMask = workSymbol.symbolMask;
                if ((symbolMask & symbolId.symbolId) == 0)
                {
                    return true;
                }
                else if (!workSymbol.fixedSymbol)
                {
                    workSymbol.symbolMask = workSymbol.symbolMask - symbolId.symbolId;
                    return true;
                }
            }

            for (int i = maxMajorDrums; i < lineId.indexOnDrum.Length; i++)
            {
                SymbolInWork workSymbol = matrix.symbols[i][lineId.indexOnDrum[i]];
                if (!workSymbol.fixedSymbol)
                {
                    workSymbol.symbolMask = workSymbol.symbolMask - symbolId.symbolId;
                }
            }

            return false;
        }

        bool CheckForAvaliableLine(GenSymbolId symbolId, GenLineId lineId, MatrixInWork processMatrix)
        {
            int maxAgreeDrumIndex = symbolId.symbolConfig.combinations[0].count;
            for (int i = 0; i < maxAgreeDrumIndex; i++)
            {
                int indexOnDrum = lineId.lineConfig.indexOnDrum[i];
                uint cellMask = processMatrix.symbols[i][indexOnDrum].symbolMask;
                if ((cellMask & symbolId.symbolId) == 0)
                    return false;
            }

            return true;
        }

        bool CheckLineToPrint(SlotLine lineId, GenSymbolId symbolId, SymbolCombination symbolCombination, MatrixInWork matrix)
        {
            int maxMajorSymbols = symbolCombination.count;
            for (int i = 0; i < lineId.indexOnDrum.Length; i++)
            {
                int indexInDrum = lineId.indexOnDrum[i];
                SymbolInWork matrixSymbol = matrix.symbols[i][indexInDrum];

                if (i < maxMajorSymbols)
                {
                    if ((matrixSymbol.symbolMask & symbolId.symbolId) == 0)
                        return false;
                }
                else
                {
                    if ((matrixSymbol.symbolMask & symbolId.symbolId) != 0 && matrixSymbol.fixedSymbol)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        bool PrintSymbolLineToMatrix(SlotLine lineId, GenSymbolId symbolId, SymbolCombination symbolCombination, MatrixInWork matrix)
        {
            int maxMajorSymbols = symbolCombination.count;
            for (int i = 0; i < lineId.indexOnDrum.Length; i++)
            {
                int indexInDrum = lineId.indexOnDrum[i];
                SymbolInWork matrixSymbol = matrix.symbols[i][indexInDrum];


                if (i < maxMajorSymbols)
                {
                    if ((matrixSymbol.symbolMask & symbolId.symbolId) == 0)
                        return false;

                    if (!matrixSymbol.fixedSymbol)
                    {
                        matrixSymbol.symbolMask = symbolId.symbolId;
                        matrixSymbol.fixedSymbol = true;
                    }
                }
                else
                {
                    if ((matrixSymbol.symbolMask & symbolId.symbolId) != 0)
                    {
                        if (matrixSymbol.fixedSymbol)
                            return false;

                        matrixSymbol.symbolMask = matrixSymbol.symbolMask - symbolId.symbolId;
                    }
                }
            }

            return true;
        }

        bool AddLineCombination(List<SlotLine> lineCombination, GenSymbolId symbolId, List<SymbolCombination> symbolCombinations, MatrixInWork matrix)
        {
            int combIndex = 0;
            foreach (var curLine in lineCombination)
            {
                if (!PrintSymbolLineToMatrix(curLine, symbolId, symbolCombinations[combIndex++], matrix))
                    return false;
            }
            return true;
        }



        List<List<SlotLine>> FindLineCombinations(int lineCount, List<GenLineId> allLines)
        {
            List<List<SlotLine>> result = new List<List<SlotLine>>();
            var digits = new int[lineCount];

            do
            {
                uint resultComb = 0;
                bool identicalExist = false;
                foreach (var curDigit in digits)
                {
                    if ((resultComb & allLines[curDigit].lineId) != 0)
                        identicalExist = true;

                    resultComb = resultComb | allLines[curDigit].lineId;
                }
                if (identicalExist)
                    continue;

                List<SlotLine> newLineList = new List<SlotLine>();
                foreach (var curDigit in digits)
                {
                    newLineList.Add(allLines[curDigit].lineConfig);
                }

                result.Add(newLineList);
            } while (Increment(allLines.Count, digits));

            return result;
        }

        bool Increment(int baseNumber, int[] digits)
        {
            int i = digits.Length - 1;
            while (i >= 0 && digits[i] == baseNumber - 1)
            {
                digits[i] = 0;
                i -= 1;
            }

            if (i < 0)
            {
                return false;
            }
            digits[i] += 1;
            return true;
        }

        void CalculateRewardBySymbol(int index, GenSymbolId symbolId, FinalSpinReward inputReward)
        {
            /*List<GenLineId> linesForIndex = new List<GenLineId>();
            foreach (var curLine in lineIdToGenLine.Values)
            {
                if (curLine.lineConfig.indexOnDrum[0] == index)
                    linesForIndex.Add(curLine);
            }
            Shuffle(linesForIndex);

            List<uint> excludeLines = new List<uint>();

            while (true)
            {
                uint linesCombination = 0b10101; // calculate by need reward and possible lines
                CalculateRewardByLines(index, symbolId, linesCombination, linesForIndex);
            }
            */

        }

        void CalculateRewardByLines(int index, GenSymbolId symbolId, uint linesCombination, List<GenLineId> linesForIndex)
        {
            int combinationCount = symbolId.symbolConfig.combinations.Length;
            int curCombinationIndex = 0;
            int[] currentCombinations = new int[combinationCount];
            while (true)
            {
                int i = 0;
                foreach (var curLineId in linesForIndex)
                {
                    if ((curLineId.lineId & linesCombination) != 0)
                    {
                        currentCombinations[i++] = curCombinationIndex;
                    }
                }

                curCombinationIndex++;
            }

            //symbolId.symbolConfig.combinations[0].
        }

        void Shuffle<T>(T[] objects)
        {
            for (int t = 0; t < objects.Length; t++)
            {
                T tmp = objects[t];
                int r = Random.Range(t, objects.Length);
                objects[t] = objects[r];
                objects[r] = tmp;
            }
        }

        void Shuffle<T>(List<T> objects)
        {
            for (int t = 0; t < objects.Count; t++)
            {
                T tmp = objects[t];
                int r = Random.Range(t, objects.Count);
                objects[t] = objects[r];
                objects[r] = tmp;
            }
        }


        /*uint GetRandomDefaultSymbol()
        {
            var defaultSymbols =
                SlotsGame.Instance.SlotMachineConfig.symbols.Where(
                    curSymbol => curSymbol.type == SlotSymbolType.Default);

            int randomIndex = UnityEngine.Random.Range(0, defaultSymbols.Count());
            uint foundId = 0;
            if (symbolTagsToId.TryGetValue(defaultSymbols.ElementAt(randomIndex).tag, out foundId))
            {
                return foundId;
            }

            return 0;
        }*/
    }
}
