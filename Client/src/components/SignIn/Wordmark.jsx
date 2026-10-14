// "singular express" wordmark, same bold/light split as HRConnect's sign-in page.
const Wordmark = ({ className = "" }) => (
  <span className={`wordmark ${className}`} aria-label="SingularExpress">
    <span className="wordmark-bold">singular</span>
    <span className="wordmark-light">express</span>
  </span>
);

export default Wordmark;
