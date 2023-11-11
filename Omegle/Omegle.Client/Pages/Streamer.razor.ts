export var peerConnection;
export var stream: MediaStream;
export var dotnet: any;

export async function setup(dotnetInput: any) {
    dotnet = dotnetInput;

    try {
        stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
        const videoElement = document.querySelector('video#localVideo') as HTMLVideoElement;
        videoElement.srcObject = stream;
        videoElement.autoplay = true;
        videoElement.play().catch(console.error);
    } catch (error) {
        console.error('Error accessing media devices.', error);
    }
}

export async function setupPeerConnection(connectionId: string) {
    peerConnection = new RTCPeerConnection();
    peerConnection.oniceconnectionstatechange = (event) => {
        console.log(`ICE state: ${peerConnection.iceConnectionState}`);
        console.log('ICE state change event: ', event);
    };
    peerConnection.onicecandidate = (event) => {
        if (event.candidate) {
            dotnet.invokeMethodAsync('SendIce', connectionId, JSON.stringify(event.candidate));
        }
    };

    peerConnection.ontrack = (event) => {
        const [remoteStream] = event.streams;
        const videoElement = document.querySelector('video#remoteVideo') as HTMLVideoElement;
        videoElement.srcObject = remoteStream;
        videoElement.autoplay = true;
        videoElement.play().catch(console.error);
    };

    stream.getTracks().forEach(track => {
        peerConnection.addTrack(track, stream);
    });
}
export async function createOffer(connectionId: string) : Promise<string> {
  
    await setupPeerConnection(connectionId);

   
    const offer = await peerConnection.createOffer();
    await peerConnection.setLocalDescription(offer);

    var offerString = JSON.stringify(offer);
    return offerString;
}


export async function acceptOffer(connectionId: string, offerString: string): Promise<string> {
    await setupPeerConnection(connectionId);

    var offer = JSON.parse(offerString);


    await peerConnection.setRemoteDescription(new RTCSessionDescription(offer));
    const answer = await peerConnection.createAnswer();
    await peerConnection.setLocalDescription(answer);
    
    // Send the answer back to the signaling server to be passed to the share page
    var answerString = JSON.stringify(answer);
    return answerString;
}

export async function acceptAnswer(connectionId: string, answerString: string) {
    console.log("answer");
    console.log(answerString);

    var answer = JSON.parse(answerString);
    await peerConnection.setRemoteDescription(new RTCSessionDescription(answer));

    // Send the answer back to the signaling server to be passed to the share page
    var answerString = JSON.stringify(answer);
    return answerString;
}

export async function acceptIce(connectionId: string, iceString: string) {
    console.log("reccc ice");
    console.log(iceString);
    var ice = JSON.parse(iceString);
    await peerConnection.addIceCandidate(new RTCIceCandidate(ice));


}

