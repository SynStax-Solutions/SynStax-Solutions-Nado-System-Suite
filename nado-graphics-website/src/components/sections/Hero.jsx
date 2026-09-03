import Eyebrow from '../ui/Eyebrow'
import Button from '../ui/Button'
import './Hero.css'

export default function Hero({
  brand = 'NADO',
  tagline = "A studio for brands that don't whisper.",
  eyebrow = 'Taking on 3 new clients this quarter',
  heading = <>Design that <em>refuses</em> to blend in.</>,
  lead = "We're a graphic design studio for brands who'd rather get noticed than get comfortable. Identity, packaging and motion, built warm and bold on purpose.",
}) {
  return (
    <>
      {/* INTRO SECTION */}
      <section className="intro-section" id="intro">
        <div className="intro-bg">
          <div className="intro-blob intro-blob-one"></div>
          <div className="intro-blob intro-blob-two"></div>
          <div className="intro-blob intro-blob-three"></div>
        </div>

        <div className="intro-inner">
          <div className="intro-logo">
            {brand}<span>.</span>
          </div>

          <p className="intro-tag">
            {tagline}
          </p>

          <a href="#hero" className="scroll-cue">
            <span>Scroll</span>
            <span className="chev"></span>
          </a>
        </div>
      </section>


      {/* MAIN HERO */}
      <section className="hero" id="hero">
        <div className="hero-grad"></div>

        <div className="hero-shape shape-tri"></div>
        <div className="hero-shape shape-circ"></div>
        <div className="hero-shape shape-square"></div>

        <div className="wrap hero-inner">
          <Eyebrow>
            {eyebrow}
          </Eyebrow>

          <h1>
            {heading}
          </h1>

          <p className="lead">
            {lead}
          </p>

          <div className="hero-ctas">
            <Button href="#work" variant="primary">
              See the work
            </Button>

            <Button href="#contact" variant="ghost">
              Book a call
            </Button>
          </div>
        </div>
      </section>
    </>
  )
}