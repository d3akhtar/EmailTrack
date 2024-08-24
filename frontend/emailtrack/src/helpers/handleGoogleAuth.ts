const handleGoogleAuth = (teamJoinLink? : string) => {
    console.log("handleGoogleAuth");
    const callbackUrl = process.env.REACT_APP_EMAILSTATS_ENDPOINT + "/external-login" // `${window.location.href.split("#")[0]}`;
    const google_clientId = "104076046651-vurufau3auce3i9pvmhjnf2ia19go761.apps.googleusercontent.com";
    const targetUrl = `https://accounts.google.com/o/oauth2/auth?redirect_uri=${encodeURIComponent(
        callbackUrl
      )}&response_type=code&client_id=${google_clientId}&state=${teamJoinLink}&access_type=offline&scope=openid%20email%20profile https%3A//www.googleapis.com/auth/gmail.metadata&include_granted_scopes=true`;
    window.location.href = targetUrl;
}

export default handleGoogleAuth;