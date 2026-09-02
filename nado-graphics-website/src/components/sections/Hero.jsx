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
      <section className="intro-section snap" id="intro">
        <svg className="intro-svg" viewBox="0 0 800 800" preserveAspectRatio="xMidYMid slice" xmlns="http://www.w3.org/2000/svg" role="presentation">
          <defs>
            <radialGradient id="g1" cx="30%" cy="30%" r="70%">
              <stop offset="0%" stopColor="#E07A2C" stopOpacity="0.95" />
              <stop offset="100%" stopColor="#B8371C" stopOpacity="0" />
            </radialGradient>
            <radialGradient id="g2" cx="70%" cy="65%" r="65%">
              <stop offset="0%" stopColor="#FAF4E7" stopOpacity="0.8" />
              <stop offset="100%" stopColor="#E07A2C" stopOpacity="0" />
            </radialGradient>
            <radialGradient id="g3" cx="75%" cy="20%" r="55%">
              <stop offset="0%" stopColor="#B8371C" stopOpacity="0.85" />
              <stop offset="100%" stopColor="#D9A417" stopOpacity="0" />
            </radialGradient>
          </defs>
          <circle className="blob b1" cx="250" cy="260" r="360" fill="url(#g1)" />
          <circle className="blob b2" cx="560" cy="540" r="300" fill="url(#g2)" />
          <circle className="blob b3" cx="600" cy="170" r="260" fill="url(#g3)" />
        </svg>
        <div className="intro-inner">
          <div className="intro-logo">{brand}<span>.</span></div>
          <p className="intro-tag">{tagline}</p>
          <a href="#hero" className="scroll-cue">
            <span>Scroll</span>
            <span className="chev"></span>
          </a>
        </div>
      </section>

      <section className="hero snap" id="hero">
        <div className="hero-grad"></div>
        <div className="hero-shape shape-tri"></div>
        <div className="hero-shape shape-circ"></div>
        <div className="hero-shape shape-square"></div>
        <div className="wrap hero-inner">
          <Eyebrow>{eyebrow}</Eyebrow>
          <h1>{heading}</h1>
          <p className="lead">{lead}</p>
          <div className="hero-ctas">
            <Button href="#work" variant="primary">See the work</Button>
            <Button href="#contact" variant="ghost">Book a call</Button>
          </div>
        </div>
      </section>
    </>
  )
}