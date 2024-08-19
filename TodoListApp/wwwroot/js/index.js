const inputBox = document.getElementById("input-box");
const listContainer = document.getElementById("list-container");
const completedCounter = document.getElementById("completed-counter");
const uncompletedCounter = document.getElementById("uncompleted-counter");
const scoreCounter = document.getElementById("score-counter");

function updateCounters() {
    const completedTasks = document.querySelectorAll(".completed").length;
    const uncompletedTasks = listContainer.querySelectorAll("li:not(.completed)").length;

    completedCounter.textContent = completedTasks;
    uncompletedCounter.textContent = uncompletedTasks;
}

async function addTask(check, newItem) {
    const task = inputBox.value.trim();
    if (!task) {
        alert("Please write down a task");
        console.log("no task added");
        return;
    }

    const li = document.createElement("li");
    li.innerHTML = `
    <label>
      <input type="checkbox">
      <span class="task-lbl">${task}</span>
    </label>
    <span class="edit-btn">Edit</span>
    <span class="delete-btn">Delete</span>
    `;

    listContainer.appendChild(li);

    inputBox.value = " ";

    const checkbox = li.querySelector("input");
    const editBtn = li.querySelector(".edit-btn");
    const taskSpan = li.querySelector("span");
    const deleteBtn = li.querySelector(".delete-btn");
    const label = li.querySelector("label");

    checkbox.addEventListener("click", function () {
        li.classList.toggle("completed", checkbox.checked);
        if (checkbox.checked) {
            updateCompleted(1);
            scoreCounter.textContent = parseInt(scoreCounter.textContent, 10) + 10;
        } else {
            updateCompleted(0);
            scoreCounter.textContent = parseInt(scoreCounter.textContent, 10) - 10;
        }
        updateCounters();
        updateScore(scoreCounter.textContent);
        updateItem(label.id, taskSpan.textContent, checkbox.checked);
    });

    if (check) {
        checkbox.checked = true;
        li.classList.add("completed");
    }

    editBtn.addEventListener("click", function () {
        const update = prompt("Edit task:", taskSpan.textContent);
        if (update !== null) {
            taskSpan.textContent = update;
            li.classList.remove("completed");
            if (checkbox.checked) {
                scoreCounter.textContent = parseInt(scoreCounter.textContent, 10) - 10;
            }
            checkbox.checked = false;
            updateCounters();
            updateScore(scoreCounter.textContent);
            updateCompleted(0);
            updateItem(label.id, taskSpan.textContent, checkbox.checked);
        }
    });

    deleteBtn.addEventListener("click", function () {
        if (confirm("Are you sure you want to delete this task?")) {
            li.remove();
            updateCounters();
            deleteItem(label.id);
        }
    });
    updateCounters();
    if (newItem) {
        addItem(task, checkbox.checked);
    }
}

inputBox.addEventListener("keyup", function (event) {
    if (event.key === "Enter") {
        addTask();
    }
});