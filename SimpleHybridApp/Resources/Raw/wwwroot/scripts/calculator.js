let display = null;
let currentInput = '';
let previousInput = '';
let currentOperator = null;
let newNumberFlag = false;

document.addEventListener('DOMContentLoaded', () => {
    display = document.getElementById('display');
    clearDisplay();
});


window.addEventListener(
    "HybridWebViewMessageReceived",
    function (e) {
        if (e.detail.message === "Celebrate")
            triggerConfetti();
    });

function appendNumber(number) {
    // If starting a new number after an operator or result, reset currentInput
    if (newNumberFlag) {
        currentInput = number;
        newNumberFlag = false;
        updateDisplay();
        return;
    }
    
    // Prevent multiple decimal points
    if (number === '.' && currentInput.includes('.')) {
        return;
    }
    
    // Handle the case when current input is '0'
    if (currentInput === '0' && number !== '.') {
        currentInput = number;
    } else {
        currentInput += number;
    }
    updateDisplay();
}

function appendOperator(operator) {
    // Don't allow operator if no input
    if (currentInput === '' && previousInput === '') return;
    
    // If we have a previous calculation, use the current input as the start of a new calculation
    if (newNumberFlag) {
        previousInput = currentInput;
        currentInput = '';
        currentOperator = operator;
        updateDisplay();
        return;
    }
    
    // If we already have a previous input, calculate the result before setting new operator
    if (previousInput !== '' && currentInput !== '' && currentOperator !== null) {
        calculate();
        previousInput = currentInput;
        currentInput = '';
    } else {
        // Store current input as previous and prepare for new input
        previousInput = currentInput;
        currentInput = '';
    }
    
    currentOperator = operator;
    updateDisplay();
}

function calculate() {
    if (previousInput === '' || currentInput === '' || currentOperator === null) {
        return;
    }
    
    const prev = parseFloat(previousInput);
    const current = parseFloat(currentInput);
    let result = 0;
    
    switch (currentOperator) {
        case '+':
            result = prev + current;
            break;
        case '-':
            result = prev - current;
            break;
        case '*':
            result = prev * current;
            break;
        case '/':
            if (current === 0) {
                clearDisplay();
                display.value = 'Error';
                return;
            }
            result = prev / current;
            break;
    }
    
    const expression = `${prev} ${currentOperator} ${current} = ${result}`;
    addToHistory(expression);
    
    // Format the result to prevent excessive decimal places
    currentInput = Number(result.toFixed(10)).toString();
    previousInput = '';
    currentOperator = null;
    newNumberFlag = true;
    updateDisplay();

    // Trigger confetti if result is 25
    if (result === 25) {
        window.HybridWebView.SendRawMessage('Celebrate');
    }
}

function triggerConfetti() {
    confetti({
        particleCount: 100,
        spread: 70,
        origin: { y: 0.6 }
    });
}

function addToHistory(entry) {
    const historyList = document.getElementById('history-list');
    const listItem = document.createElement('li');
    listItem.textContent = entry;
    historyList.appendChild(listItem);
}

function clearDisplay() {
    currentInput = '';
    previousInput = '';
    currentOperator = null;
    newNumberFlag = false;
    updateDisplay();
}

function updateDisplay() {
    if (!display) return;
    
    if (previousInput === '') {
        // Show just the current input if no previous input exists
        display.value = currentInput || '0';
    } else {
        // Show the complete expression
        display.value = `${previousInput} ${currentOperator || ''} ${currentInput || ''}`.trim();
    }
}
