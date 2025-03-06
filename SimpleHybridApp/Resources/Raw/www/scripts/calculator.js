let display = null;
let currentInput = '';
let previousInput = '';
let currentOperator = null;
let newNumberFlag = false;

document.addEventListener('DOMContentLoaded', () => {
    display = document.getElementById('display');
    clearDisplay();
});

function appendNumber(number) {
    if (newNumberFlag) {
        currentInput = '';
        newNumberFlag = false;
    }
    
    // Prevent multiple decimal points
    if (number === '.' && currentInput.includes('.')) {
        return;
    }
    
    currentInput += number;
    updateDisplay();
}

function appendOperator(operator) {
    if (currentInput === '' && previousInput === '') return;
    
    if (currentInput === '') {
        currentOperator = operator;
        return;
    }
    
    if (previousInput !== '') {
        calculate();
    }
    
    previousInput = currentInput;
    currentOperator = operator;
    newNumberFlag = true;
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
    
    currentInput = result.toString();
    previousInput = '';
    currentOperator = null;
    updateDisplay();
    newNumberFlag = true;
}

function clearDisplay() {
    currentInput = '';
    previousInput = '';
    currentOperator = null;
    newNumberFlag = false;
    updateDisplay();
}

function updateDisplay() {
    if (display) {
        display.value = currentInput || '0';
    }
}
