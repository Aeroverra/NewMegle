


export var localPeerConnection;
export var remotePeerConnection;


export async function acceptIce(dotnet: any, connectionId: string, iceString: string, destinationLocal: boolean) {
    console.log("reccc ice");
    console.log(iceString);
    var ice = JSON.parse(iceString);
    if (destinationLocal == true) {
        await localPeerConnection.addIceCandidate(new RTCIceCandidate(ice));
    } else {
        await remotePeerConnection.addIceCandidate(new RTCIceCandidate(ice));
    }

}

export async function acceptAnswer(dotnet: any, connectionId: string, answerString: string) {
    console.log("reccc");
    console.log(answerString);
    var answer = JSON.parse(answerString);
    
    await localPeerConnection.setRemoteDescription(new RTCSessionDescription(answer));

}



export async function acceptOffer(dotnet: any, connectionId: string, offerString: string): Promise<string> {
    remotePeerConnection = new RTCPeerConnection();
    remotePeerConnection.oniceconnectionstatechange = (event) => {
        console.log(`ICE state: ${remotePeerConnection.iceConnectionState}`);
        console.log('ICE state change event: ', event);
    };
    remotePeerConnection.onicecandidate = (event) => {
        if (event.candidate) {
            dotnet.invokeMethodAsync('SendIce', connectionId, JSON.stringify(event.candidate), true);
        }
    };

    remotePeerConnection.ontrack = (event) => {
        const [remoteStream] = event.streams;
        const videoElement = document.querySelector('video#remoteVideo') as HTMLVideoElement;
        videoElement.srcObject = remoteStream;
        videoElement.autoplay = true;
        videoElement.play().catch(console.error);
    };
    var offer = JSON.parse(offerString);


    await remotePeerConnection.setRemoteDescription(new RTCSessionDescription(offer));
    const answer = await remotePeerConnection.createAnswer();
    await remotePeerConnection.setLocalDescription(answer);

    // Send the answer back to the signaling server to be passed to the share page
    var answerString = JSON.stringify(answer);
    console.log(answerString);
    return answerString;
}







export async function createStream() {
    var result = await createPeerConnection();
    return result;
}

export async function getLocalStream(): Promise<MediaStream> {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
        const videoElement = document.querySelector('video#localVideo') as HTMLVideoElement;
        videoElement.srcObject = stream;
        videoElement.autoplay = true;
        videoElement.play().catch(console.error);
        return stream;
    } catch (error) {
        console.error('Error accessing media devices.', error);
    }
}

export async function createPeerConnection(): Promise<string> {
    const localStream: MediaStream = await getLocalStream();
    localPeerConnection = new RTCPeerConnection();

    localStream.getTracks().forEach(track => {
        localPeerConnection.addTrack(track, localStream);
    });

    const offer = await localPeerConnection.createOffer();
    await localPeerConnection.setLocalDescription(offer);

    var offerString = JSON.stringify(offer);
    console.log(offerString);
    return JSON.stringify(offer);
}
