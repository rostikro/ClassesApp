import { useEffect, useState } from "react";
import axios from "axios"
import { useParams } from "react-router-dom";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faClipboardList } from '@fortawesome/free-solid-svg-icons'

function TaskDetails() {
    const {id} = useParams();

    const [taskDetails, setTaskDetails] = useState(null);
    const [submissionUrl, setSubmissionUrl] = useState('');

    useEffect(() => {
        axios.defaults.headers.common["Authorization"] = 'Bearer ' + localStorage.getItem("token");
        fetchTaskDetails();
    }, []);

    const fetchTaskDetails = async () => {
        axios.get('https://localhost:7066/api/Tasks/getTask/'+id).then(response => {
            // handle success
            setTaskDetails(response.data);
            setSubmissionUrl(response.data.submissionUrl);
          });
    }

    const submitTask = async () => {
        await axios({
            method: 'post',
            url: 'https://localhost:7066/api/Submissions/submit',
            data: {
                taskId: id,
                url: submissionUrl,
            },
          });

        fetchTaskDetails();
    }

    return (
        <div className="task-container">
            <div className="task-text col-8">
                <div className="task-main">
                    <h1 className="task-name">{taskDetails?.title}</h1>
                    <p className="teacher">{taskDetails?.teacher}</p>
                    <p className="grade">{taskDetails?.grade == null ? taskDetails?.maxGrade + ' points' : taskDetails?.grade + '/' + taskDetails?.maxGrade}</p>
                </div>
                <p className="description">{taskDetails?.description}</p>
            </div>
            <div className="task-submission-container">
                <div className="work-flex">
                    <p className="work-title">Your work</p>
                    <p className="graded">{taskDetails?.submissionUrl != null ? taskDetails?.grade == null ? 'Submited' : 'Graded' : 'Assigned'}</p>
                </div>
                <p className="due-to">Due to: {taskDetails?.dueDate}</p>
                <textarea type='text' className="textarea" placeholder="Type you solution here..." onChange={(e) => setSubmissionUrl(e.target.value)} value={submissionUrl}></textarea>
                <button className="submit-btn" onClick={submitTask}>{taskDetails?.submissionUrl == null ? 'Submit' : 'Resumbit'}</button>
            </div>
        </div>
    )
}

export default TaskDetails;