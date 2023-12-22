async function sendPostRequest() 
{
    try 
    {
        const url = "https://localhost:7146/Bert";
        const data = { 
            text: document.getElementById("text").innerText,
            question: document.getElementById("question").value 
        };

        console.log("Request sent!")

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
        console.log(response);
        document.getElementById("answer").innerText = response.answer;
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
    const container = document.getElementById("entry-container");
 
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