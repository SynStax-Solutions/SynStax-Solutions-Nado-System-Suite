import Button from '../ui/Button'
import './CTA.css'

export default function CTA({
  heading = "Got a brand that deserves better? Let's make something warm and loud.",
  email = 'hello@nado.design',
}) {
  return (
    <section className="contact-section snap" id="contact">
      <div className="wrap">
        <div className="contact">
          <div className="contact-dot"></div>
          <h2>{heading}</h2>
          <Button href={`mailto:${email}`} variant="primary">{email}</Button>
        </div>
      </div>
    </section>
  )
}