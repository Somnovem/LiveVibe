import { useEffect, useState } from 'react';
import './App.css';

function App() {
    const [events, setEvents] = useState();
    const baseUrl = "http://localhost:5000";
    
    useEffect(() => {
        populateEvents();
    }, []);

    const contents2 = events === undefined
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started. See <a href="https://aka.ms/jspsintegrationreact">https://aka.ms/jspsintegrationreact</a> for more details.</em></p>
        : <table className="table table-striped" aria-labelledby="tableLabel">
            <thead>
                <tr>
                    <th>ID</th>
                    <th>Title</th>
                    <th>Description</th>
                    <th>Organizer</th>
                    <th>Category</th>
                    <th>City</th>
                    <th>Location</th>
                    <th>Time</th>
                    <th>Image</th>
                </tr>
            </thead>
            <tbody>
                {events.map(event =>
                    <tr key={event.id}>
                        <td>{event.id}</td>
                        <td>{event.title}</td>
                        <td>{event.description}</td>
                        <td>{event.organizer}</td>
                        <td>{event.category}</td>
                        <td>{event.city}</td>
                        <td>{event.location}</td>
                        <td>{event.time}</td>
                        <td><img src={baseUrl + event.imageUrl} alt={event.title} style={{width: '100px', height: '100px', objectFit: 'cover'}}/></td>
                    </tr>
                )}
            </tbody>
        </table>;

    return (
        <div>
            <h1 id="tableLabel">Livevibe reached.</h1>
            <p>This component demonstrates fetching data from the server.</p>
            {contents2}
        </div>
    );

    async function populateEvents() {
        const response = await fetch('/api/events/featured');
        if (response.ok) {
            const data = await response.json();
            console.log(data);
            setEvents(data);
        }
    }
}

export default App;