async function sendPostRequest() 
{
    statusDiv = document.getElementById("status");
    statusDiv.innerText = "Status: Sent";
    statusDiv.classList.add("sent");
    statusDiv.classList.remove("inactive");
    statusDiv.classList.remove("recieved");
    try 
    {
        const url = "https://localhost:7146/Bert";
        const questionContainer = document.getElementById("question-container");
        const questionsBlocks = questionContainer.querySelectorAll(".question-block");
        let questions = [];
        let answerDivs = [];
        questionsBlocks.forEach(block => {
            questions.push(block.querySelector(".question").value);
            answerDivs.push(block.querySelector(".answer"));
        });
        
        const data = { 
            Text: document.getElementById("text").innerText,
            Questions: questions
        };

        console.log("Request sent!");
        response = await fetch(url, {
            method: "POST",
            body: JSON.stringify(data),
            headers: 
            {
                "Content-Type": "application/json"
            }
        })
        console.log(response);
        response = await response.json();
        for (let i = 0; i < response.answers.length; i++)
        {
            answerDivs[i].innerText = response.answers[i];
        }
        statusDiv.innerText = "Status: Recieved";
        statusDiv.classList.add("recieved");
        statusDiv.classList.remove("sent");

    }
    catch(e) 
    {
        e => console.log(e);
    }
}

function uploadFile() 
{
    const input = document.getElementById("file");
    const file = input.files[0];

    if (file)
    {
        const reader = new FileReader();

        reader.onload = function(e) 
        {
            const text = e.target.result;
            document.getElementById("text").innerText = text;
        }

        reader.readAsText(file);
    }
}

function addEntry() 
{
    const container = document.getElementById("question-container");
 
    const newEntry = document.createElement("div");
    newEntry.classList.add("question-block");
 
    const labelDiv = document.createElement("div");
    labelDiv.classList.add("label");
    labelDiv.innerText = "Question:"
    newEntry.appendChild(labelDiv);

    const textArea = document.createElement("textarea");
    textArea.classList.add("question");
    newEntry.appendChild(textArea);
 
    const answerDiv = document.createElement("div");
    answerDiv.classList.add("answer");
    newEntry.appendChild(answerDiv);
 
    const removeBtn = document.createElement("button");
    removeBtn.classList.add("remove-btn");
    removeBtn.innerText = "Remove";
    removeBtn.onclick = function() {
     removeEntry(removeBtn);
    }

    newEntry.appendChild(removeBtn);
 
    container.appendChild(newEntry);
}
 
function removeEntry(btn) {
    const entryDiv = btn.parentNode;
    entryDiv.parentNode.removeChild(entryDiv);
}